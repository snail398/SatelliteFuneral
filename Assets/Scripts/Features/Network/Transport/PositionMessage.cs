using ProtoBuf;

namespace Network.Transport
{
    [GameMessage]
    [ProtoContract]
    public class PositionMessage
    {
        [ProtoMember(1)]
        public float X;
        [ProtoMember(2)]
        public float Y;
        [ProtoMember(3)]
        public float Z;
        [ProtoMember(4)]
        public ulong SteamId;
    }
}