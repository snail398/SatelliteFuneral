using Network.Transport;
using Server.Spawn;

namespace Server.MessageHandlers
{
    public class SpawnMessageHandler : MessageHandler<SpawnMessage>
    {
        private readonly SpawnService _SpawnService;

        public SpawnMessageHandler(SpawnService spawnService)
        {
            _SpawnService = spawnService;
        }

        protected override void MessageReceived(SpawnMessage message)
        {
            _SpawnService.SpawnPlayer(message.PlayerId);
        }
    }
}