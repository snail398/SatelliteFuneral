using ProtoBuf;

namespace Network.Transport
{
    [GameMessage]
    [ProtoContract]
    public class GameMessageContainer
    {
        [ProtoMember(1)]
        public int MessageId { get; set; }
        [ProtoMember(2)]
        public byte[] MessageData { get; set; }
    }
}