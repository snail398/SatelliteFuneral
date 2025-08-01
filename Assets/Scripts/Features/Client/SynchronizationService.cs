using System.Collections.Generic;
using DependencyInjection;
using Shared;
using Steamworks;
using UnityEngine;

namespace Client
{
    public class SynchronizationService : ILoadableService, IUnloadableService
    {
        private readonly IMessageSender _MessageSender;
        

        private Dictionary<ulong, TestPlayerController> _SpawnedClients = new Dictionary<ulong, TestPlayerController>();

        public SynchronizationService(IMessageSender messageSender)
        {
            _MessageSender = messageSender;
        }
        
        void ILoadableService.Load() { }

        public void ReceiveSnapshot(GameSnapshot snapshot)
        {
            Debug.Log($"receive snapshot");
            //TODO: send message about receiving ??
            if (snapshot == null)
                return;
            if (snapshot.SpawnSnapshots != null)
            {
                foreach (var spawnSnapshot in snapshot.SpawnSnapshots)
                {
                    if (spawnSnapshot.Spawned && !_SpawnedClients.ContainsKey(spawnSnapshot.SteamId))
                    {
                        var player = Resources.Load<TestPlayerController>("Player");
                        var inst = Object.Instantiate(player);
                        var isLocal = spawnSnapshot.SteamId == SteamUser.GetSteamID().m_SteamID;
                        inst.gameObject.name = $"Player::STEAMID::{spawnSnapshot.SteamId}::{(isLocal ? "Local" : "Remote")}";
                        inst.Setup(_MessageSender, isLocal);
                        _SpawnedClients.Add(spawnSnapshot.SteamId, inst);
                    }
                }
            }

            if (snapshot.PositionSnapshots != null)
            {
                foreach (var positionSnapshot in snapshot.PositionSnapshots)
                {
                    if (positionSnapshot.SteamId != SteamUser.GetSteamID().m_SteamID)
                    {
                        _SpawnedClients[positionSnapshot.SteamId].transform.position = new Vector3(positionSnapshot.X,
                            positionSnapshot.Y, positionSnapshot.Z);
                    }
                }
            }
        }

        void IUnloadableService.Unload()
        {
            
        }
    }
}