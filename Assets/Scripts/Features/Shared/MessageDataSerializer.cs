using System;
using System.Collections.Generic;
using System.Linq;
using Network.Transport;
using Utils;

namespace Shared
{
    public class MessageDataSerializer
    {
        private readonly Dictionary<int, Type> _MessageIdToTypeCache;
        private readonly Dictionary<Type, int> _MessageTypeToIdCache;
        
        public MessageDataSerializer() {
            var allTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(_ => _.GetTypes());
            _MessageIdToTypeCache = new Dictionary<int, Type>();
            _MessageTypeToIdCache = new Dictionary<Type, int>();
            foreach (var type in allTypes) {
                if (type.CustomAttributes.All(a => a.AttributeType != typeof(GameMessageAttribute)))
                    continue;
                var id = MessageIdHelper.GetMessageId(type);
                _MessageIdToTypeCache.Add(id, type);
                _MessageTypeToIdCache.Add(type, id);
            }
        }
        
        public byte[] Serialize(object messageData) {
            var data = ProtobufHelper.Serialize(messageData);
            return data;
        }

        public object Deserialize(byte[] bytes, int messageId) {
            var type = _MessageIdToTypeCache[messageId];
            var data = ProtobufHelper.Deserialize(bytes, type);
            return data;
        }
        
        public int GetMessageTypeId(Type type) {
            if (!_MessageTypeToIdCache.TryGetValue(type, out var typeId))
                throw new Exception($"message id for type {type} not found");
            return typeId;
        }
    }
}