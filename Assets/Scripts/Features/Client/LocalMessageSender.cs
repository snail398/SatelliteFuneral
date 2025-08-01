using Server;

namespace Client
{
    public class LocalMessageSender : IMessageSender
    {
        private readonly GameServer _GameServer;

        public LocalMessageSender(GameServer gameServer)
        {
            _GameServer = gameServer;
        }

        public void SendMessage<T>(T messageData)
        {
            _GameServer.ReceiveMessage(messageData);
        }
    }
}