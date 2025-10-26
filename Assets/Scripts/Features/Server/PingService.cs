using System.Collections.Generic;
using Shared;
using Steamworks;

namespace Server
{
    public class PingService : ISnapshotDataProvider<List<PingSnapshot>>
    {
        private GameServer _GameServer;

        public PingService(GameServer gameServer)
        {
            _GameServer = gameServer;
        }

        List<PingSnapshot> ISnapshotDataProvider<List<PingSnapshot>>.SnapshotData
        {
            get
            {
                var snapshot = new List<PingSnapshot>();
                for (int i = 0; i < _GameServer.PlayerInGame.Count; i++)
                {
                    var identity = _GameServer.PlayerInGame[i];
                    SteamNetworkingMessages.GetSessionConnectionInfo(ref identity, out SteamNetConnectionInfo_t pConnectionInfo, out SteamNetConnectionRealTimeStatus_t pQuickStatus);
                    snapshot.Add(new PingSnapshot()
                    {
                        SteamId = identity.GetSteamID64(),
                        Ping = pQuickStatus.m_nPing,
                    });
                }

                return snapshot;
            }
        }

    }
}