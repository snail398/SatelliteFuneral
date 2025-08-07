using ProtoBuf;

namespace Network.Transport
{
    [GameMessage]
    [ProtoContract]
    public class TakeItemMessage
    {
        [ProtoMember(1)]
        public uint ItemId;
    }
}