using Network.Transport;

namespace Client
{
    public class LocalGameServerProvider
    {
        private readonly MessageProcessor _MessageProcessor;

        public LocalGameServerProvider(MessageProcessor messageProcessor)
        {
            _MessageProcessor = messageProcessor;
        }

        public void ReceiveMessage(object message)
        {
            _MessageProcessor.ProcessMessage(message);
        }
    }
}