using System;
using System.IO;
using ProtoBuf;

namespace Utils
{
    public static class ProtobufHelper
    {
        public static byte[] Serialize(object obj)
        {
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static object Deserialize(byte[] data, Type type)
        {
            using (var ms = new MemoryStream(data))
            {
                return Serializer.Deserialize(type, ms);
            }
        }
        
        public static T Deserialize<T>(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                return Serializer.Deserialize<T>(ms);
            }
        }
    }
}