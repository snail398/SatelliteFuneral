namespace Client
{
    public interface IServerProvider
    {
        public int CurrentTimestamp { get; }
        void SetCurrentTick(int serverTick);
    }
}