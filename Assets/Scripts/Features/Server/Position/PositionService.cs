using System.Collections.Generic;
using Shared;
using Unity.Mathematics;

namespace Server.Position
{
    public class PositionService : ISnapshotDataProvider<List<PositionSnapshot>>
    {
        private Dictionary<ulong, float3> _Positions = new Dictionary<ulong, float3>();
        private Dictionary<ulong, quaternion> _Rotations = new Dictionary<ulong, quaternion>();
        
        public void SetPosition(ulong user, float3 position, quaternion rotation)
        {
            _Positions[user] = position;
            _Rotations[user] = rotation;
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
                        Position = kvp.Value,
                        Rotation = _Rotations[kvp.Key]
                    });
                }

                return snapshots;
            }
        }
    }
}