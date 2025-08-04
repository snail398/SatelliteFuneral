namespace Client
{
    public interface IServerProvider
    {
        public long CurrentTimestamp { get; }
        void SetCurrentTick(long serverTick);
    }
}