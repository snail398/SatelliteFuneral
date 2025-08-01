namespace Server
{
    public interface ISnapshotDataProvider<T> where T : class {
        T SnapshotData { get; }
    }
}