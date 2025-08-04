namespace Network.Transport
{
    public abstract class MessageHandler<T>: MessageHandlerWrapper where T: class {
        public override void MessageObjectReceived(object concreteMessage, ulong steamId) {
            MessageReceived(concreteMessage as T, steamId);
        }

        protected abstract void MessageReceived(T message, ulong steamId);
    }
}