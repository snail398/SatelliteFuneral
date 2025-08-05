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
            _PositionService.SetPosition(message.SteamId, new float3(message.XPos, message.YPos, message.ZPos), new quaternion(message.XRot, message.YRot, message.ZRot, message.WRot));
        }
    }
}