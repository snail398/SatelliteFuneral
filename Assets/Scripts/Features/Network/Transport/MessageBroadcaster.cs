using System;
using System.Collections.Generic;
using Client;
using Shared;
using Steamworks;
using GameServer = Server.GameServer;

namespace Network.Transport
{
    public class MessageBroadcaster
    {
        private readonly GameServer _GameServer;
        private readonly MessageDataSerializer _MessageDataSerializer;
        private readonly LocalGameServerProvider _LocalGameServerProvider;

        public MessageBroadcaster(GameServer gameServer, LocalGameServerProvider localGameServerProvider, MessageDataSerializer messageDataSerializer)
        {
            _GameServer = gameServer;
            _LocalGameServerProvider = localGameServerProvider;
            _MessageDataSerializer = messageDataSerializer;
        }

        public void BroadcastMessage(object messageData, List<SteamNetworkingIdentity> receivers)
        {
            for (int i = 0; i < receivers.Count; i++)
            {
                var receiver = receivers[i];
                if (_GameServer.Host.Equals(receiver))
                {
                    //SendLocal
                    _LocalGameServerProvider.ReceiveMessage(messageData);
                }
                else
                {
                    var serializedMessage = _MessageDataSerializer.Serialize(messageData);
                    var messageContainer = new GameMessageContainer()
                    {
                        MessageData = serializedMessage,
                        MessageId = _MessageDataSerializer.GetMessageTypeId(messageData.GetType())
                    };
                    var data = _MessageDataSerializer.Serialize(messageContainer);
                    unsafe
                    {
                        fixed (byte* ptr = data)
                        {
                            SteamNetworkingMessages.SendMessageToUser(ref receiver, (IntPtr)ptr, (uint)data.Length, 0, 0);
                        }
                    }
                }
            }
        }
    }
}