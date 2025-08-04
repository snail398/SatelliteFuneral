using System.Collections.Generic;
using Network.Transport;
using ProtoBuf;

namespace Shared
{
    [GameMessage]
    [ProtoContract]
    public class GameSnapshot
    {
        [ProtoMember(1)] 
        public uint ServerTick;
        [ProtoMember(2)] 
        public uint LastHandledInput;
        [ProtoMember(3)]
        public List<SpawnSnapshot> SpawnSnapshots;
        [ProtoMember(4)]
        public List<PositionSnapshot> PositionSnapshots;
    }
}