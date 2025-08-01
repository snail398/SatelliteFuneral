using Network.Transport;

namespace Client.MessageHandlers
{
    public class GameSnapshotMessageHandler : MessageHandler<GameSnapshotMessage>
    {
        private readonly SynchronizationService _SynchronizationService;

        public GameSnapshotMessageHandler(SynchronizationService synchronizationService)
        {
            _SynchronizationService = synchronizationService;
        }

        protected override void MessageReceived(GameSnapshotMessage message)
        {
            _SynchronizationService.ReceiveSnapshot(message.GameSnapshot);
        }
    }
}