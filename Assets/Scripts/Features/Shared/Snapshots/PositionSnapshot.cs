using ProtoBuf;

namespace Shared
{
    [ProtoContract]
    public class PositionSnapshot
    {
        [ProtoMember(1)]
        public ulong SteamId;       
        [ProtoMember(2)]
        public SharedVector3 Position;
        [ProtoMember(3)]
        public SharedVector4 Rotation;
    }
}