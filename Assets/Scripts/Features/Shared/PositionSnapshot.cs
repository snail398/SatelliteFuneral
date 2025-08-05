using ProtoBuf;
using Unity.Mathematics;

namespace Shared
{
    [ProtoContract]
    public class PositionSnapshot
    {
        [ProtoMember(1)]
        public ulong SteamId;
        [ProtoMember(2)]
        public float3 Position;
        [ProtoMember(3)]
        public quaternion Rotation;
    }
}