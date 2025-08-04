namespace Network.Transport
{
    public abstract class MessageHandlerWrapper {       
        public abstract void MessageObjectReceived(object concreteMessage, ulong steamId);
    }
}