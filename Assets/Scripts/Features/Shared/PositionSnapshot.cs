using ProtoBuf;

namespace Shared
{
    [ProtoContract]
    public class PositionSnapshot
    {
        [ProtoMember(1)]
        public ulong SteamId;
        [ProtoMember(2)]
        public float X;
        [ProtoMember(3)]
        public float Y;
        [ProtoMember(4)]
        public float Z;
    }
}