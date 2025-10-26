using ProtoBuf;

namespace Shared
{
    [ProtoContract]
    public class PingSnapshot
    {
        [ProtoMember(1)]
        public ulong SteamId;   
        [ProtoMember(2)]
        public int Ping;        
    }
}