using System.Collections.Generic;
using Client.Inventory;
using Features;
using Server.Position;
using Shared;
using Unity.Mathematics;
using UnityEngine;

namespace Server
{
    public class ItemService : ISnapshotDataProvider<List<ItemSnapshot>>
    {
        private readonly PositionService _PositionService;
        private readonly SpawnItemSettings _SpawnItemSettings;
        private readonly GameServer _GameServer;
        
        private Dictionary<uint, ItemState> _ItemStates = new Dictionary<uint, ItemState>();
        //item to uint
        private Dictionary<uint, ulong> _ItemPossessionMap = new Dictionary<uint, ulong>();
        //unit to item
        private Dictionary<ulong, uint> _UnitPossessionMap = new Dictionary<ulong, uint>();
        private uint _IdAllocator;
        
        List<ItemSnapshot> ISnapshotDataProvider<List<ItemSnapshot>>.SnapshotData
        {
            get
            {
                var snapshots = new List<ItemSnapshot>(10);
                foreach (var kvp in _ItemStates)
                {
                    snapshots.Add(new ItemSnapshot()
                    {
                        ItemId = kvp.Key,
                        ItemName = kvp.Value.ItemName,
                        Position = kvp.Value.Position,
                        Rotation = kvp.Value.Rotation,
                        Owner = kvp.Value.Owner,
                        Possession = kvp.Value.Possession,
                    });
                }

                return snapshots;
            }
        }
        
        public ItemService(PositionService positionService, SpawnItemSettings spawnItemSettings, GameServer gameServer)
        {
            _PositionService = positionService;
            _SpawnItemSettings = spawnItemSettings;
            _GameServer = gameServer;

            foreach (var itemSpawner in _SpawnItemSettings._ItemSpawners)
            {
                RegisterItem(itemSpawner.ItemName, itemSpawner.transform.position, itemSpawner.transform.rotation, _GameServer.Host.GetSteamID().m_SteamID);
            }
        }

        public void RegisterItem(string itemName, float3 position, quaternion rotation, ulong owner)
        {
            var itemState = new ItemState()
            {
                ItemId = _IdAllocator++,
                ItemName = itemName,
                Position = position,
                Rotation = rotation,
                Owner = owner,
                Possession = 0,
            };
            _ItemStates.Add(itemState.ItemId, itemState);
        }

        public void TakeItem(uint itemId, ulong possessorId)
        {
            if (!_ItemStates.TryGetValue(itemId, out ItemState itemState))
            {
                Debug.LogError($"Item with id {itemId} does not exist");
                return;
            }

            if (_ItemPossessionMap.TryGetValue(itemId, out var currentPossessor))
            {
                Debug.LogError($"Cant take an item that is currently in use by {currentPossessor}");
                return;
            }
            if (_UnitPossessionMap.TryGetValue(possessorId, out var possessedItem))
            {
                DropItem(possessedItem, possessorId);
            }

            itemState.Possession = possessorId;
            _ItemPossessionMap[itemId] = possessorId;
            _UnitPossessionMap[possessorId] = itemId;
        }
        
        public void DropItem(uint itemId, ulong possessorId)
        {
            if (!_ItemStates.TryGetValue(itemId, out ItemState itemState))
            {
                Debug.LogError($"Item with id {itemId} does not exist");
                return;
            }

            itemState.Possession = 0;
            _ItemPossessionMap.Remove(itemId);
            _UnitPossessionMap.Remove(possessorId);
            var possessorPosition = _PositionService.GetPosition(possessorId);
            itemState.Position = possessorPosition;
        }
    }
}