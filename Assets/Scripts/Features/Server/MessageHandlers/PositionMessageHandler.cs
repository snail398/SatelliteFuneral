using System.Numerics;
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
            _PositionService.SetPosition(message.SteamId, new Vector3(message.X, message.Y, message.Z));
        }
    }
}