using ProtoBuf;

namespace Shared
{
    [ProtoContract]
    public class ItemSnapshot
    {
        [ProtoMember(1)]
        public uint ItemId;
        [ProtoMember(2)]
        public SharedVector3 Position;
        [ProtoMember(3)]
        public SharedVector4 Rotation;
        [ProtoMember(4)]
        public string ItemName;
        [ProtoMember(5)]
        public ulong Owner;
        [ProtoMember(6)]
        public ulong Possession;
    }
}