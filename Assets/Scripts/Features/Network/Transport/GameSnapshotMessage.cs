using ProtoBuf;
using Shared;

namespace Network.Transport
{
    [GameMessage]
    [ProtoContract]
    public class GameSnapshotMessage
    {
        [ProtoMember(1)]
        public GameSnapshot GameSnapshot { get; set; }
    }
}