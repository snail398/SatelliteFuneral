using ProtoBuf;

namespace Network.Transport
{
    [GameMessage]
    [ProtoContract]
    public class SpawnMessage
    {
        [ProtoMember(1)]
        public ulong PlayerId;
    }
}