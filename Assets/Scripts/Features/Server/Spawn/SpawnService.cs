using System.Collections.Generic;
using System.Linq;
using DependencyInjection;
using Shared;
using Steamworks;
using UnityEngine;
using Utils;

namespace Server.Spawn
{
    public class SpawnService : ILoadableService, ISnapshotDataProvider<List<SpawnSnapshot>>
    {
        private readonly GameServer _GameServer;
        private readonly SignalBus _SignalBus;

        private readonly List<SpawnState> _SpawnStates = new List<SpawnState>();

        List<SpawnSnapshot> ISnapshotDataProvider<List<SpawnSnapshot>>.SnapshotData
        {
            get
            {
                var snapshots = new List<SpawnSnapshot>(10);
                foreach (var spawnState in _SpawnStates)
                {
                    snapshots.Add(new SpawnSnapshot()
                    {
                        SteamId = spawnState.SteamId,
                        Spawned = spawnState.Spawned,
                    });
                }

                return snapshots;
            }
        }
        
        public SpawnService(GameServer gameServer, SignalBus signalBus)
        {
            _GameServer = gameServer;
            _SignalBus = signalBus;

            var alreadyConnectedUsers = _GameServer.ConnectedUsers;
            foreach (var connectedUser in alreadyConnectedUsers)
            {
                AddPlayerSpawnState(connectedUser.GetSteamID());
            }
            _SignalBus.Subscribe<PlayerConnectedSignal>(OnPlayerConnected, this);
        }
        
        public void Load() { }
        
        private void OnPlayerConnected(PlayerConnectedSignal signal)
        {
            AddPlayerSpawnState(signal.CSteamID);
        }

        private void AddPlayerSpawnState(CSteamID steamID)
        {
            var spawnState = new SpawnState()
            {
                SteamId = steamID.m_SteamID,
                Spawned = false,
            };
            _SpawnStates.Add(spawnState);
        }

        public void SpawnPlayer(ulong playerId)
        {
            Debug.Log($"Trying to spawn player {playerId}");
            var state = _SpawnStates.FirstOrDefault(_ => _.SteamId == playerId);
            if (state == null)
            {
                Debug.Log($"dont have connected player {playerId}");
                return;
            }
            state.Spawned = true;
        }
    }
}