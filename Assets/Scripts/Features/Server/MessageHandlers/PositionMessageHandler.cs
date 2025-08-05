using Network.Transport;
using Server.Position;

namespace Server.MessageHandlers
{
    public class PositionMessageHandler : MessageHandler<PositionMessage>
    {
        private PositionService _PositionService;

        public PositionMessageHandler(PositionService positionService)
        {
            _PositionService = positionService;
        }
        
        protected override void MessageReceived(PositionMessage message, ulong steamId)
        {
            _PositionService.SetPosition(message.SteamId, message.Position, message.Rotation);
        }
    }
}