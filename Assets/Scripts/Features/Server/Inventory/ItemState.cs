using Unity.Mathematics;

namespace Server
{
    public class ItemState
    {
        public uint ItemId;
        public string ItemName;
        public float3 Position;
        public quaternion Rotation;
        public ulong Owner;
        public ulong Possession;
    }
}