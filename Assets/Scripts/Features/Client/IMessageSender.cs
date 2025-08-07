namespace Client
{
    public interface IMessageSender
    {
        void SendMessage<T>(T messageData, bool reliable = false);

    }
}