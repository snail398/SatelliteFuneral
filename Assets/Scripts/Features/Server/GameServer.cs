using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Client;
using DependencyInjection;
using Network.Lobby;
using Network.Transport;
using Server.Position;
using Shared;
using Steamworks;
using UnityEngine;
using Utils;
using SpawnService = Server.Spawn.SpawnService;

namespace Server
{
    public class GameServer
    {
        private readonly GameLobbyServer _GameLobbyServer;
        private readonly ITimerProvider _TimerProvider;
        private readonly LocalGameServerProvider _LocalGameServerProvider;
        
        private List<SteamNetworkingIdentity> _ConnectedUsers = new List<SteamNetworkingIdentity>();
        private SteamNetworkingIdentity _Host;
        private Container _Container;
        private SignalBus _SignalBus;
        private MessageProcessor _MessageProcessor;
        private MessageDataSerializer _MessageDataSerializer;
        private object _UpdateTimer;

        public List<SteamNetworkingIdentity> ConnectedUsers => _ConnectedUsers;
        public SteamNetworkingIdentity Host => _Host;
        protected Callback<SteamNetworkingMessagesSessionRequest_t> _SessionRequest;

        public GameServer(GameLobbyServer gameLobbyServer, ITimerProvider timerProvider, LocalGameServerProvider localGameServerProvider)
        {
            //TODO its not nessessary to have game lobby server neer GameServer
            _GameLobbyServer = gameLobbyServer;
            _TimerProvider = timerProvider;
            _LocalGameServerProvider = localGameServerProvider;
            foreach (var userInLobby in  _GameLobbyServer.UsersInLobby)
            {
                Debug.Log($"GAMESERVER::user already in lobby: {userInLobby}");
                var networkIdentity = new SteamNetworkingIdentity();
                networkIdentity.SetSteamID(userInLobby);
                _ConnectedUsers.Add(networkIdentity);
            }
            
            _GameLobbyServer.OnUserJoin += OnUserJoin;
            _GameLobbyServer.OnUserLeft += OnUserLeft;
            
            
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
            
            _UpdateTimer = _TimerProvider.CreateTimer(ListenForMessages, 50, 50);

            _SessionRequest = Callback<SteamNetworkingMessagesSessionRequest_t>.Create(OnSessionRequest);
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
            Debug.Log($"server received {receivedCount} messages");
            for (int i = 0; i < receivedCount; i++)
            {
                if (messagePtrs[i] == IntPtr.Zero)
                    continue;

                SteamNetworkingMessage_t msg = Marshal.PtrToStructure<SteamNetworkingMessage_t>(messagePtrs[i]);
                Debug.Log($"server received message from: {msg.m_identityPeer.GetSteamID().ToString()}");

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
                _MessageProcessor.ProcessMessage(message);
                // Освободи память
                SteamNetworkingMessage_t.Release(messagePtrs[i]);
            }
        }

        public void CreateGame(CSteamID host)
        {
            _Host = new SteamNetworkingIdentity();
            _Host.SetSteamID(host);
            CreateAndRegisterServices();
        }

        private void CreateAndRegisterServices()
        {
            _Container.RegisterSingleton<SynchronizationService>().Resolve();
            _Container.RegisterSingleton<SpawnService>().RegisterInstanceInterfaces();
            _Container.RegisterSingleton<PositionService>().RegisterInstanceInterfaces();
            //TODO: add here gameplay server features
        }
        private void OnUserLeft(CSteamID steamID)
        {
            Debug.Log($"GAMESERVER::user left: {steamID}");

            for (int i = 0; i < _ConnectedUsers.Count; i++)
            {
                if (_ConnectedUsers[i].GetSteamID() == steamID)
                {
                    _ConnectedUsers.RemoveAt(i);
                    _SignalBus.FireSignal(new PlayerLeftSignal(steamID));
                    break;
                }
            }
        }

        private void OnUserJoin(CSteamID steamID)
        {
            Debug.Log($"GAMESERVER::user joined: {steamID}");
            var networkIdentity = new SteamNetworkingIdentity();
            networkIdentity.SetSteamID(steamID);
            _ConnectedUsers.Add(networkIdentity);           
            _SignalBus.FireSignal(new PlayerConnectedSignal(steamID));

        }

        public void ReceiveMessage<T>(T messageData)
        {
            _MessageProcessor.ProcessMessage(messageData);
        }
    }
}