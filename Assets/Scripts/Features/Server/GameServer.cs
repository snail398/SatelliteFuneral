using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Client;
using DependencyInjection;
using Network.Lobby;
using Network.Transport;
using Server.Input;
using Server.Position;
using Shared;
using Steamworks;
using Utils;
using Debug = UnityEngine.Debug;
using InputService = Server.Input.InputService;
using SpawnService = Server.Spawn.SpawnService;

namespace Server
{
    public class GameServer
    {
        private const int ServerTickRateMs = 50;
        
        private readonly GameLobbyServer _GameLobbyServer;
        private readonly ITimerProvider _TimerProvider;
        private readonly LocalGameServerProvider _LocalGameServerProvider;
        
        private Dictionary<ulong, SteamNetworkingIdentity> _ConnectedUsers = new Dictionary<ulong, SteamNetworkingIdentity>();
        private SteamNetworkingIdentity _Host;
        private Container _Container;
        private SignalBus _SignalBus;
        private MessageProcessor _MessageProcessor;
        private MessageDataSerializer _MessageDataSerializer;
        private UnityEventProvider _UnityEventProvider;
        private object _UpdateTimer;
        private uint _ServerTick;
        private long _CurrentTimestamp;
        
        public long CurrentTimestamp => _CurrentTimestamp;
        
        private SynchronizationService _SynchronizationService;
        private InputService _InputService;

        private List<SteamNetworkingIdentity> _PlayerInGame = new List<SteamNetworkingIdentity>();
        public List<SteamNetworkingIdentity> PlayerInGame => _PlayerInGame;
        public SteamNetworkingIdentity Host => _Host;
        protected Callback<SteamNetworkingMessagesSessionRequest_t> _SessionRequest;

        private Stopwatch _Stopwatch; 
        
        public GameServer(GameLobbyServer gameLobbyServer, ITimerProvider timerProvider, LocalGameServerProvider localGameServerProvider, UnityEventProvider unityEventProvider)
        {
            //TODO its not nessessary to have game lobby server neer GameServer
            _GameLobbyServer = gameLobbyServer;
            _TimerProvider = timerProvider;
            _LocalGameServerProvider = localGameServerProvider;
            _UnityEventProvider = unityEventProvider;
            foreach (var userInLobby in  _GameLobbyServer.UsersInLobby)
            {
                Debug.Log($"GAMESERVER::user already in lobby: {userInLobby}");
                var networkIdentity = new SteamNetworkingIdentity();
                networkIdentity.SetSteamID(userInLobby);
                _ConnectedUsers.Add(userInLobby.m_SteamID, networkIdentity);
            }
            
            _GameLobbyServer.OnUserJoin += OnUserJoin;
            _GameLobbyServer.OnUserLeft += OnUserLeft;

            _LocalGameServerProvider.SetGameServer(this);
            _Container = new Container();
            _Container.RegisterInstance(_Container);
            _Container.RegisterInstance(this);
            _Container.RegisterInstance(_LocalGameServerProvider);
            _SignalBus = new SignalBus();
            _Container.RegisterInstance(_SignalBus);
            _MessageProcessor = _Container.RegisterSingleton<MessageProcessor>().Resolve<MessageProcessor>();
            _MessageDataSerializer = new MessageDataSerializer();
            _Container.RegisterInstance(_MessageDataSerializer);
            _Container.RegisterSingleton<MessageBroadcaster>().Resolve();
            _Container.RegisterInstance(_TimerProvider, typeof(ITimerProvider));
            
            _UnityEventProvider.OnFixedUpdate += UpdateServer;
            _UnityEventProvider.OnUpdate += UpdateInternal;

            _SessionRequest = Callback<SteamNetworkingMessagesSessionRequest_t>.Create(OnSessionRequest);
        }

        private void UpdateInternal()
        {
            _CurrentTimestamp = _Stopwatch.ElapsedMilliseconds;
        }

        private void UpdateServer()
        {
            ListenForMessages();
            _InputService.ProcessInput();
            // ProcessInputs();
            // SimulatePhysics(fixedDeltaTime);
            _SynchronizationService.BroadcastSnapshots(_CurrentTimestamp);
            _ServerTick++;
            // Debug.LogError($"GAMESERVER::UpdateInternal: Current timestamp: {_CurrentTimestamp} server tick : {_ServerTick} calc: {_CurrentTimestamp / 50}");
            // _CurrentTimestamp = _ServerTick * ServerTickRateMs;
        }

        private void OnSessionRequest(SteamNetworkingMessagesSessionRequest_t param)
        {
            Debug.LogError($"AcceptSessionWithUser: {param.m_identityRemote.GetSteamID().m_SteamID}");
            SteamNetworkingMessages.AcceptSessionWithUser(ref param.m_identityRemote);
        }

        private void ListenForMessages()
        {
            int channel = 0;
            int maxMessages = 16;
            IntPtr[] messagePtrs = new IntPtr[maxMessages];

            int receivedCount = SteamNetworkingMessages.ReceiveMessagesOnChannel(channel, messagePtrs, maxMessages);
            for (int i = 0; i < receivedCount; i++)
            {
                if (messagePtrs[i] == IntPtr.Zero)
                    continue;

                SteamNetworkingMessage_t msg = Marshal.PtrToStructure<SteamNetworkingMessage_t>(messagePtrs[i]);

                if (msg.m_identityPeer.Equals(_Host))
                {
                    Debug.LogWarning("Server received host message via SteamNetworkingMessage");
                    SteamNetworkingMessage_t.Release(messagePtrs[i]);
                    continue;
                }
                byte[] buffer = new byte[msg.m_cbSize];
                Marshal.Copy(msg.m_pData, buffer, 0, buffer.Length);
                var messageContainer = ProtobufHelper.Deserialize<GameMessageContainer>(buffer);
                var message = _MessageDataSerializer.Deserialize(messageContainer.MessageData, messageContainer.MessageId);
                _MessageProcessor.ProcessMessage(message, msg.m_identityPeer.GetSteamID().m_SteamID);
                // Освободи память
                SteamNetworkingMessage_t.Release(messagePtrs[i]);
            }
        }

        public void CreateGame(CSteamID host)
        {
            _Stopwatch = Stopwatch.StartNew();
            _Host = new SteamNetworkingIdentity();
            _Host.SetSteamID(host);
            _PlayerInGame.Add(_Host);
            CreateAndRegisterServices();
        }

        public void Welcome(ulong id)
        {
            var identity = new SteamNetworkingIdentity();
            identity.SetSteamID64(id);
            _PlayerInGame.Add(identity);
        }
        
        private void CreateAndRegisterServices()
        {
            _InputService = _Container.RegisterSingleton<InputService>().Resolve<InputService>();
            _SynchronizationService = _Container.RegisterSingleton<SynchronizationService>().Resolve<SynchronizationService>();
            _Container.RegisterSingleton<SpawnService>().RegisterInstanceInterfaces();
            _Container.RegisterSingleton<PositionService>().RegisterInstanceInterfaces();
            _Container.RegisterSingleton<ItemService>().RegisterInstanceInterfaces();
            //TODO: add here gameplay server features
        }
        private void OnUserLeft(CSteamID steamID)
        {
            Debug.Log($"GAMESERVER::user left: {steamID}");

            _ConnectedUsers.Remove(steamID.m_SteamID);
            _SignalBus.FireSignal(new PlayerLeftSignal(steamID));
        }

        private void OnUserJoin(CSteamID steamID)
        {
            Debug.Log($"GAMESERVER::user joined: {steamID}");
            var networkIdentity = new SteamNetworkingIdentity();
            networkIdentity.SetSteamID(steamID);
            _ConnectedUsers.Add(steamID.m_SteamID, networkIdentity);           
            _SignalBus.FireSignal(new PlayerConnectedSignal(steamID));

        }

        public void ReceiveMessage<T>(T messageData)
        {
            _MessageProcessor.ProcessMessage(messageData, _Host.GetSteamID().m_SteamID);
        }
    }
}