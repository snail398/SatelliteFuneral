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
        public List<SpawnSnapshot> SpawnSnapshots;
        [ProtoMember(2)]
        public List<PositionSnapshot> PositionSnapshots;
    }
}