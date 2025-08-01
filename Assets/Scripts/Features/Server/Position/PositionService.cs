using System.Collections.Generic;
using System.Numerics;
using Shared;

namespace Server.Position
{
    public class PositionService : ISnapshotDataProvider<List<PositionSnapshot>>
    {
        private Dictionary<ulong, Vector3> _Positions = new Dictionary<ulong, Vector3>();
        
        public void SetPosition(ulong user, Vector3 position)
        {
            _Positions[user] = position;
        }

        List<PositionSnapshot> ISnapshotDataProvider<List<PositionSnapshot>>.SnapshotData
        {
            get
            {
                var snapshots = new List<PositionSnapshot>(10);
                foreach (var kvp in _Positions)
                {
                    snapshots.Add(new PositionSnapshot()
                    {
                        SteamId = kvp.Key,
                        X = kvp.Value.X,
                        Y = kvp.Value.Y,
                        Z = kvp.Value.Z,
                    });
                }

                return snapshots;
            }
        }
    }
}