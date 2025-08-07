using Network.Transport;

namespace Server.MessageHandlers
{
    public class RegisterItemMessageHandler : MessageHandler<RegisterItemMessage>
    {
        private readonly ItemService _ItemService;

        public RegisterItemMessageHandler(ItemService itemService)
        {
            _ItemService = itemService;
        }

        protected override void MessageReceived(RegisterItemMessage message, ulong steamId)
        {
            _ItemService.RegisterItem(message.ItemName, message.Position, message.Rotation, steamId);
        }
    }
}