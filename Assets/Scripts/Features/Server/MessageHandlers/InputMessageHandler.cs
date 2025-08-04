using Network.Transport;

namespace Server.MessageHandlers
{
    public class InputMessageHandler : MessageHandler<InputMessage>
    {
        protected override void MessageReceived(InputMessage message, ulong steamId)
        {
            
        }
    }
}