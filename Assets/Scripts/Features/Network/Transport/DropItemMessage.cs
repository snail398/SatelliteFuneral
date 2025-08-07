using ProtoBuf;

namespace Network.Transport
{
    [GameMessage]
    [ProtoContract]
    public class DropItemMessage
    {
        [ProtoMember(1)]
        public uint ItemId;
    }
}