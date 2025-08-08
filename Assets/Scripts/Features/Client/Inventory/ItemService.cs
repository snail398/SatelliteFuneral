using System.Collections.Generic;
using System.Linq;
using DependencyInjection;
using Network.Transport;
using Shared;
using Steamworks;
using UnityEngine;
using Utils;

namespace Client.Inventory
{
    public class ItemService : IUnloadableService, ISnapshotDataUpdateReceiver<List<ItemSnapshot>>
    {
        private const int PICKUP_RANGE = 1;
        
        private readonly UnityEventProvider _UnityEventProvider;
        private readonly IMessageSender _MessageSender;
        private readonly UnitService _UnitService;
        
        private readonly Dictionary<uint, ItemView> _Items = new Dictionary<uint, ItemView>();
        private readonly Dictionary<uint, ulong> _PossessionMap = new Dictionary<uint, ulong>();

        public ItemService(UnityEventProvider unityEventProvider, IMessageSender messageSender, UnitService unitService)
        {
            _UnityEventProvider = unityEventProvider;
            _MessageSender = messageSender;
            _UnitService = unitService;

            _UnityEventProvider.OnUpdate += Update;
        }

        private void Update()
        {
            foreach (var itemKvp in _Items)
            {
                itemKvp.Value.SetInteractable(false);
            }
            {
                Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
                Ray ray = Camera.main.ScreenPointToRay(screenCenter);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, PICKUP_RANGE, 1 << LayerMask.NameToLayer("Item")))
                {
                    if (hit.transform.GetComponent<ItemView>())
                    {
                        var view = hit.transform.GetComponent<ItemView>();
                        if (!_PossessionMap.TryGetValue(view.ItemId, out var possessor))
                        {
                            view.SetInteractable(true);
                        }
                    }
                }
            }


            if (Input.GetKeyDown(KeyCode.E))
            { 
                Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
                Ray ray = Camera.main.ScreenPointToRay(screenCenter);
                RaycastHit hit;
                
                if (Physics.Raycast(ray, out hit, PICKUP_RANGE, 1 <<LayerMask.NameToLayer("Item")))
                {
                    if (hit.transform.GetComponent<ItemView>())
                    {
                        var view = hit.transform.GetComponent<ItemView>();
                        _MessageSender.SendMessage( new TakeItemMessage()
                        {
                            ItemId = view.ItemId,
                        });                        
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                var possessed = _PossessionMap.Any(_ => _.Value == SteamUser.GetSteamID().m_SteamID);
                if (possessed)
                {
                    var kvp = _PossessionMap.FirstOrDefault(_ => _.Value == SteamUser.GetSteamID().m_SteamID);
                    _MessageSender.SendMessage(new DropItemMessage()
                    {
                        ItemId = kvp.Key,
                    });
                }
            }
        }

        public void Unload()
        {
            _UnityEventProvider.OnUpdate -= Update;
        }

        public void ReceiveSnapshotDataUpdate(List<ItemSnapshot> data, long timestamp)
        {
            if (data != null)
            {
                foreach (var itemSnapshot in data)
                {
                    if (!_Items.TryGetValue(itemSnapshot.ItemId, out var itemView))
                    {
                        itemView = SpawnItem(itemSnapshot);
                    }

                    if (itemSnapshot.Possession.Equals(0))
                    {
                        if (_PossessionMap.TryGetValue(itemSnapshot.ItemId, out var possessor))
                        {
                            var unit = _UnitService.GetUnit(possessor);
                            unit?.DropItem(itemView);
                            _PossessionMap.Remove(itemSnapshot.ItemId);
                        }
                        itemView.SetPosition(itemSnapshot.Position, itemSnapshot.Rotation, timestamp);
                    }
                    else
                    {
                        if (!_PossessionMap.TryGetValue(itemSnapshot.ItemId, out var possessor))
                        {
                            var unit = _UnitService.GetUnit(itemSnapshot.Possession);
                            unit?.TakeItem(itemView);
                            itemView.SetInteractable(false);
                            _PossessionMap[itemSnapshot.ItemId] = itemSnapshot.Possession;
                        }
                    }
                }
            }
        }

        private ItemView SpawnItem(ItemSnapshot itemSnapshot)
        {
            ItemView itemView;
            var prefab = Resources.Load<ItemView>(itemSnapshot.ItemName);
            itemView = Object.Instantiate(prefab, itemSnapshot.Position, itemSnapshot.Rotation);
            _Items[itemSnapshot.ItemId] = itemView;
            itemView.Initialize(itemSnapshot.Owner.Equals(SteamUser.GetSteamID().m_SteamID), itemSnapshot.ItemId);
            return itemView;
        }
    }
}