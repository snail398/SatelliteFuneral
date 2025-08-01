namespace Network.Transport
{
    public abstract class MessageHandler<T>: MessageHandlerWrapper where T: class {
        public override void MessageObjectReceived(object concreteMessage) {
            MessageReceived(concreteMessage as T);
        }

        protected abstract void MessageReceived(T message);
    }
}