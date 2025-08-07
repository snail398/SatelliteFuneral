using Network.Transport;

namespace Server.MessageHandlers
{
    public class TakeItemMessageHandler : MessageHandler<TakeItemMessage>
    {
        private readonly ItemService _ItemService;

        public TakeItemMessageHandler(ItemService itemService)
        {
            _ItemService = itemService;
        }

        protected override void MessageReceived(TakeItemMessage message, ulong steamId)
        {
            _ItemService.TakeItem(message.ItemId, steamId);
        }
    }
}