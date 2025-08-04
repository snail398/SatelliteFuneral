using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using DependencyInjection;
using Network.Transport;
using Shared;
using Steamworks;
using Utils;
using Debug = UnityEngine.Debug;

namespace Client
{
    public class RemoteGameServerProvider : ILoadableService, IUnloadableService, IServerProvider
    {
        private ITimerProvider _TimerProvider;
        private MessageDataSerializer _MessageDataSerializer;
        private MessageProcessor _MessageProcessor;
        private UnityEventProvider _UnityEventProvider;
        private object _UpdateTimer;
        
        private long _CurrentTimestamp;
        private Callback<SteamNetworkingMessagesSessionRequest_t> _Callback;

        public long CurrentTimestamp => _CurrentTimestamp;

        private long _SyncDelta;
        private bool _Synced;
        private Stopwatch _Stopwatch;
        
        public void SetCurrentTick(long serverTimestamp)
        {
            if (!_Synced)
            {
                _Stopwatch = Stopwatch.StartNew();
            
                _SyncDelta = serverTimestamp;
                _CurrentTimestamp = serverTimestamp;
                _Synced = true;
            }
        }

        private void UpdateInternal()
        {
            if (!_Synced)
                return;
            _CurrentTimestamp = _Stopwatch.ElapsedMilliseconds + _SyncDelta;
        }

        public RemoteGameServerProvider(ITimerProvider timerProvider, MessageDataSerializer messageDataSerializer, MessageProcessor messageProcessor, UnityEventProvider unityEventProvider)
        {
            _TimerProvider = timerProvider;
            _MessageDataSerializer = messageDataSerializer;
            _MessageProcessor = messageProcessor;
            _UnityEventProvider = unityEventProvider;
            _UnityEventProvider.OnUpdate += UpdateInternal;
        }

        public void Load()
        {
            _UpdateTimer = _TimerProvider.CreateTimer(ListenForMessages, 20, 20);
            _Callback = Callback<SteamNetworkingMessagesSessionRequest_t>.Create(OnSessionRequest);
        }
        
        private void OnSessionRequest(SteamNetworkingMessagesSessionRequest_t param)
        {
            Debug.LogError($"Accept Session With Server: {param.m_identityRemote.GetSteamID().m_SteamID}");
            SteamNetworkingMessages.AcceptSessionWithUser(ref param.m_identityRemote);
        }

        private void ListenForMessages()
        {
            int channel = 0;
            int maxMessages = 16;
            IntPtr[] messagePtrs = new IntPtr[maxMessages];

            int receivedCount = SteamNetworkingMessages.ReceiveMessagesOnChannel(channel, messagePtrs, maxMessages);
            Debug.Log($"Received {receivedCount} messages");
            for (int i = 0; i < receivedCount; i++)
            {
                if (messagePtrs[i] == IntPtr.Zero)
                    continue;

                SteamNetworkingMessage_t msg = Marshal.PtrToStructure<SteamNetworkingMessage_t>(messagePtrs[i]);

                byte[] buffer = new byte[msg.m_cbSize];
                Marshal.Copy(msg.m_pData, buffer, 0, buffer.Length);
                var messageContainer = ProtobufHelper.Deserialize<GameMessageContainer>(buffer);
                var message = _MessageDataSerializer.Deserialize(messageContainer.MessageData, messageContainer.MessageId);
                _MessageProcessor.ProcessMessage(message, msg.m_identityPeer.GetSteamID().m_SteamID);
                // Освободи память
                SteamNetworkingMessage_t.Release(messagePtrs[i]);
            }
        }

        public void ReceiveMessage(object message)
        {
            
        }

        public void Unload()
        {
            _TimerProvider.StopTimer(ref _UpdateTimer);
        }
    }
}