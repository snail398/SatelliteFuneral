namespace Client
{
    public interface ISnapshotDataUpdateReceiver<TSnapshotData> {
        void ReceiveSnapshotDataUpdate(TSnapshotData data, long timestamp);
    }
}