using Network.Transport;

namespace Server.MessageHandlers
{
    public class DropItemMessageHandler : MessageHandler<DropItemMessage>
    {
        private readonly ItemService _ItemService;

        public DropItemMessageHandler(ItemService itemService)
        {
            _ItemService = itemService;
        }

        protected override void MessageReceived(DropItemMessage message, ulong steamId)
        {
            _ItemService.DropItem(message.ItemId, steamId);
        }
    }
}