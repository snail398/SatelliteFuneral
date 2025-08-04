using Network.Transport;
using UnityEngine;

namespace Server.MessageHandlers
{
    public class WelcomeMessageHandler : MessageHandler<WelcomeMessage>
    {
        private readonly GameServer _GameServer;

        public WelcomeMessageHandler(GameServer gameServer)
        {
            _GameServer = gameServer;
        }

        protected override void MessageReceived(WelcomeMessage message, ulong steamId)
        {
            Debug.LogError($"server received a welcome message: {message} from {steamId}");
            _GameServer.Welcome(steamId);
        }
    }
}