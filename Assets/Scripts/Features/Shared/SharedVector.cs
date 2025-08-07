using ProtoBuf;
using Unity.Mathematics;
using UnityEngine;

namespace Shared
{
    [ProtoContract]
    public struct SharedVector3
    {
        [ProtoMember(1)]
        public float X;
        [ProtoMember(2)]
        public float Y;
        [ProtoMember(3)]
        public float Z;
        
        public static implicit operator float3(SharedVector3 d) => new(d.X, d.Y, d.Z);
        public static implicit operator SharedVector3(float3 b) => new() { X = b.x, Y = b.y, Z = b.z };
        
        public static implicit operator Vector3(SharedVector3 d) => new(d.X, d.Y, d.Z);
        public static implicit operator SharedVector3(Vector3 b) => new() { X = b.x, Y = b.y, Z = b.z };
    }
    
    [ProtoContract]
    public struct SharedVector4
    {
        [ProtoMember(1)]
        public float X;
        [ProtoMember(2)]
        public float Y;
        [ProtoMember(3)]
        public float Z;
        [ProtoMember(4)]
        public float W;
        
        public static implicit operator quaternion(SharedVector4 d) => new quaternion(d.X, d.Y, d.Z, d.W);
        public static implicit operator SharedVector4(quaternion b) => new() {X = b.value.x, Y = b.value.y, Z = b.value.z, W = b.value.w };
        
        
        public static implicit operator Quaternion(SharedVector4 d) => new Quaternion(d.X, d.Y, d.Z, d.W);
        public static implicit operator SharedVector4(Quaternion b) => new() {X = b.x, Y = b.y, Z = b.z, W = b.w };
    }
}