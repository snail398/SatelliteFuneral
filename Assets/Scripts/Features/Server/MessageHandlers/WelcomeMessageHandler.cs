using Network.Transport;
using UnityEngine;

namespace Server.MessageHandlers
{
    public class WelcomeMessageHandler : MessageHandler<WelcomeMessage>
    {
        protected override void MessageReceived(WelcomeMessage message, ulong steamId)
        {
            Debug.LogError($"server received a welcome message: {message} from {steamId}");
        }
    }
}