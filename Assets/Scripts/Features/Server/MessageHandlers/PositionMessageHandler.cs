using Network.Transport;
using Server.Position;
using Unity.Mathematics;

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
            _PositionService.SetPosition(message.SteamId, new float3(message.Position.X, message.Position.Y, message.Position.Z), new quaternion(message.Rotation.X, message.Rotation.Y, message.Rotation.Z, message.Rotation.W));
        }
    }
}