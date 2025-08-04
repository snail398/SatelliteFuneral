namespace Client
{
    public interface IServerProvider
    {
        public uint CurrentTimestamp { get; }
        void SetCurrentTick(uint serverTick);
    }
}