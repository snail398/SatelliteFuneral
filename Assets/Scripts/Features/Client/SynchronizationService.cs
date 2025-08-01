using System.Collections.Generic;
using DependencyInjection;
using Shared;
using UnityEngine;

namespace Client
{
    public class SynchronizationService : ILoadableService, IUnloadableService
    {
        void ILoadableService.Load() { }

        private HashSet<ulong> _SpawnedClients = new HashSet<ulong>();
        
        public void ReceiveSnapshot(GameSnapshot snapshot)
        {
            Debug.Log($"receive snapshot");
            //TODO: send message about receiving ??
            foreach (var spawnSnapshot in snapshot.SpawnSnapshots)
            {
                if (spawnSnapshot.Spawned && !_SpawnedClients.Contains(spawnSnapshot.SteamId))
                {
                    var go = new GameObject($"{spawnSnapshot.SteamId}");
                    _SpawnedClients.Add(spawnSnapshot.SteamId);
                }
            }
        }

        void IUnloadableService.Unload()
        {
            
        }
    }
}