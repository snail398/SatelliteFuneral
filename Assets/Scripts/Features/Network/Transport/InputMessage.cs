using ProtoBuf;

namespace Network.Transport
{
    [GameMessage]
    [ProtoContract]
    public class InputMessage
    {
        [ProtoMember(1)]
        public InputData InputData;
    }

    [ProtoContract]
    public class InputData
    {
        [ProtoMember(1)]
        public uint InputId;
        [ProtoMember(2)]
        public float XInput;
        [ProtoMember(3)]
        public float YInput;
        [ProtoMember(4)]
        public bool JumpPressed;
    }
}