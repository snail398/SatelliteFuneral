using Network.Transport;
using Server;

namespace Client
{
    public class LocalGameServerProvider : IServerProvider
    {
        private readonly MessageProcessor _MessageProcessor;
        private GameServer _GameServer;
        public long CurrentTimestamp => _GameServer.CurrentTimestamp;
        public void SetCurrentTick(long serverTick) { }

        public LocalGameServerProvider(MessageProcessor messageProcessor)
        {
            _MessageProcessor = messageProcessor;
        }

        public void ReceiveMessage(object message)
        {
            _MessageProcessor.ProcessMessage(message, _GameServer.Host.GetSteamID().m_SteamID);
        }

        public void SetGameServer(GameServer gameServer)
        {
            _GameServer = gameServer;
        }

    }
}