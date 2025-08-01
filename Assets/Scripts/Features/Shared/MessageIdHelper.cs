using System;
using System.Collections.Generic;
using System.Linq;
using Network.Transport;

namespace Shared
{
    public static class MessageIdHelper
    {
        private static readonly Dictionary<string, int> _MessageDataTypesIds;

        static MessageIdHelper()
        {
            var messageDataTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(_ => _.GetTypes())
                // .Where(_ => _.GetCustomAttribute<GameMessageAttribute>() != null) 
                .Where(_ => _.CustomAttributes.Any(a =>
                    a.AttributeType ==
                    typeof(GameMessageAttribute))) //.GetCustomAttribute<GameMessageAttribute>() != null)
                .Select(_ => _.FullName)
                .ToArray();
            Array.Sort(messageDataTypes, StringComparer.InvariantCulture);
            _MessageDataTypesIds = messageDataTypes.ToDictionary(_ => _, _ => Array.IndexOf(messageDataTypes, _));
        }

        public static int GetMessageId(Type messageType)
        {
            return _MessageDataTypesIds[messageType.FullName]; //to
        }
    }
}