using System.Collections.Generic;
using DependencyInjection;
using Network.Transport;
using Shared;
using Steamworks;
using UnityEngine;
using Utils;

namespace Client
{
    public class SpawnService : IUnloadableService, ISnapshotDataUpdateReceiver<List<SpawnSnapshot>>
    {
        private readonly UnityEventProvider _UnityEventProvider;
        private readonly IMessageSender _MessageSender;
        private readonly IServerProvider _ServerProvider;
        private readonly UnitService _UnitService;
        
        private HashSet<ulong> _SpawnedUnits = new HashSet<ulong>();
        
        public SpawnService(UnityEventProvider unityEventProvider, IMessageSender messageSender, IServerProvider serverProvider, UnitService unitService)
        {
            _UnityEventProvider = unityEventProvider;
            _MessageSender = messageSender;
            _ServerProvider = serverProvider;
            _UnitService = unitService;
            TrySpawn();
        }

        private void TrySpawn()
        {
            _MessageSender.SendMessage(new SpawnMessage()
            {
                PlayerId = SteamUser.GetSteamID().m_SteamID,
            }, true);
        }

        public void ReceiveSnapshotDataUpdate(List<SpawnSnapshot> data, long timestamp)
        {
            if (data != null)
            {
                foreach (var spawnSnapshot in data)
                {

                    if (spawnSnapshot.Spawned && !_SpawnedUnits.Contains(spawnSnapshot.SteamId))
                    {
                        _SpawnedUnits.Add(spawnSnapshot.SteamId);
                        _UnitService.SpawnUnit(spawnSnapshot.SteamId);
                    }
                }
            }
        }

        void IUnloadableService.Unload()
        {
        }

    }
}