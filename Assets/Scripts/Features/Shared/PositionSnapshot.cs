using ProtoBuf;

namespace Shared
{
    [ProtoContract]
    public class PositionSnapshot
    {
        [ProtoMember(1)]
        public ulong SteamId;
        [ProtoMember(2)]
        public float XPos;
        [ProtoMember(3)]
        public float YPos;
        [ProtoMember(4)]
        public float ZPos;

        [ProtoMember(5)]
        public float XRot;
        [ProtoMember(6)]
        public float YRot;
        [ProtoMember(7)]
        public float ZRot;
        [ProtoMember(8)]
        public float WRot;
    }
}