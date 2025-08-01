using ProtoBuf;

namespace Network.Transport
{
    [ProtoContract]
    public class SpawnMessage
    {
        [ProtoMember(1)]
        public ulong PlayerId;
    }
}