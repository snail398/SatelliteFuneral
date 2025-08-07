using ProtoBuf;
using Shared;

namespace Network.Transport
{
    [GameMessage]
    [ProtoContract]
    public class RegisterItemMessage
    {
        [ProtoMember(1)]
        public SharedVector3 Position;
        [ProtoMember(2)]
        public SharedVector4 Rotation;
        [ProtoMember(3)]
        public string ItemName;
    }
}