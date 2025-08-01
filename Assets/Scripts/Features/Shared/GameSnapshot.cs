using System.Collections.Generic;
using ProtoBuf;

namespace Shared
{
    [ProtoContract]
    public class GameSnapshot
    {
        [ProtoMember(1)]
        public List<SpawnSnapshot> SpawnSnapshots;
    }
}