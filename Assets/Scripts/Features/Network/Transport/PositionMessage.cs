using ProtoBuf;
using Unity.Mathematics;

namespace Network.Transport
{
    [GameMessage]
    [ProtoContract]
    public class PositionMessage
    {
        [ProtoMember(1)]
        public float3 Position;
        [ProtoMember(2)]
        public quaternion Rotation;
        [ProtoMember(3)]
        public ulong SteamId;
    }
}