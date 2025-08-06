using System.Collections.Generic;
using DependencyInjection;
using Shared;
using Steamworks;
using Unity.Mathematics;
using UnityEngine;
using Utils;

namespace Client
{
    public class UnitService : IUnloadableService, ISnapshotDataUpdateReceiver<List<PositionSnapshot>>
    {
        private IMessageSender _MessageSender;
        private IServerProvider _ServerProvider;
        private UnityEventProvider _UnityEventProvider;
        
        private Dictionary<ulong, UnitController> _SpawnedUnits = new Dictionary<ulong, UnitController>();

        public UnitService(IMessageSender messageSender, IServerProvider serverProvider, UnityEventProvider unityEventProvider)
        {
            _MessageSender = messageSender;
            _ServerProvider = serverProvider;
            _UnityEventProvider = unityEventProvider;
        }

        public void SpawnUnit(ulong steamId)
        {
            var player = Resources.Load<UnitView>("Player");
            var inst = Object.Instantiate(player, new Vector3(0, 0, 0), Quaternion.identity);
            UnitController unitController = new UnitController(steamId, inst, _MessageSender , _ServerProvider, _UnityEventProvider);
            _SpawnedUnits.Add(steamId, unitController);
        }

        public void Unload()
        {
            foreach (var kvp in _SpawnedUnits)
            {
                kvp.Value.Dispose();
            }
        }

        public void ReceiveSnapshotDataUpdate(List<PositionSnapshot> data, long timestamp)
        {
            if (data != null)
            {
                foreach (var positionSnapshot in data)
                {
                    if (positionSnapshot.SteamId != SteamUser.GetSteamID().m_SteamID)
                    {
                        _SpawnedUnits[positionSnapshot.SteamId].SetPosition( new float3(positionSnapshot.XPos, positionSnapshot.YPos, positionSnapshot.ZPos), new quaternion(positionSnapshot.XRot, positionSnapshot.YRot, positionSnapshot.ZRot, positionSnapshot.WRot), timestamp);
                    }
                }
            }
        }
    }
}