﻿using System;
using System.Runtime.InteropServices;
using DependencyInjection;
using Network.Transport;
using Shared;
using Steamworks;
using UnityEngine;
using Utils;

namespace Client
{
    public class RemoteGameServerProvider : ILoadableService, IUnloadableService
    {
        private ITimerProvider _TimerProvider;
        private MessageDataSerializer _MessageDataSerializer;
        private MessageProcessor _MessageProcessor;
        private object _UpdateTimer;

        public RemoteGameServerProvider(ITimerProvider timerProvider, MessageDataSerializer messageDataSerializer, MessageProcessor messageProcessor)
        {
            _TimerProvider = timerProvider;
            _MessageDataSerializer = messageDataSerializer;
            _MessageProcessor = messageProcessor;
        }

        public void Load()
        {
            _UpdateTimer = _TimerProvider.CreateTimer(ListenForMessages, 50, 50);
            Callback<SteamNetworkingMessagesSessionRequest_t>.Create(OnSessionRequest);
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
                _MessageProcessor.ProcessMessage(message);
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