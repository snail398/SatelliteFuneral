using ProtoBuf;

namespace Shared
{
    [ProtoContract]
    public class SpawnSnapshot
    {
        [ProtoMember(1)]
        public ulong SteamId;
        [ProtoMember(2)]
        public bool Spawned;
    }
}