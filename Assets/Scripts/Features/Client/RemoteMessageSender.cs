using System;
using Network.Lobby;
using Network.Transport;
using Shared;
using Steamworks;
using UnityEngine;

namespace Client
{
    public class RemoteMessageSender : IMessageSender
    {
        private readonly LobbyData _LobbyData;
        private readonly MessageDataSerializer _MessageDataSerializer;

        public RemoteMessageSender(LobbyData lobbyData)
        {
            _LobbyData = lobbyData;
        }

        public void SendMessage<T>(T messageData)
        {
            var serializedMessage = _MessageDataSerializer.Serialize(messageData);
            var messageContainer = new GameMessageContainer()
            {
                MessageData = serializedMessage,
                MessageId = _MessageDataSerializer.GetMessageTypeId(messageData.GetType())
            };
            var data = _MessageDataSerializer.Serialize(messageContainer);
            var receiver = new SteamNetworkingIdentity();
            receiver.SetSteamID(_LobbyData.HostId);
            Debug.Log($"Sending message to: {_LobbyData.HostId}");
            unsafe
            {
                fixed (byte* ptr = data)
                {
                    SteamNetworkingMessages.SendMessageToUser(ref receiver, (IntPtr)ptr, (uint)serializedMessage.Length, 0, 0);
                }
            }
        }
    }
}