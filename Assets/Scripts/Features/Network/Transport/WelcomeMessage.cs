using ProtoBuf;

namespace Network.Transport
{
    [GameMessage]
    [ProtoContract]
    public class WelcomeMessage
    {
        [ProtoMember(1)]
        public int Data;
    }
}