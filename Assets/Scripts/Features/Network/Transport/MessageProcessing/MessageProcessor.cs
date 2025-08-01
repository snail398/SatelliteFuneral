using System;
using System.Collections.Generic;
using System.Linq;
using DependencyInjection;
using UnityEngine;

namespace Network.Transport
{
    public class MessageProcessor
    {
        private readonly Container _Container;
        
        private Dictionary<Type, Type> _MessagesHandlersMap;
        private readonly Dictionary<Type, MessageHandlerWrapper> _HandlersCache = new Dictionary<Type, MessageHandlerWrapper>();

        public MessageProcessor(Container container)
        {
            _Container = container;
            _MessagesHandlersMap = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(_ => _.GetTypes())
                .Where(_ => !_.IsAbstract && IsAssignableFromGenericDefinition(_, typeof(MessageHandler<>), out _))
                .ToDictionary(_ => {
                    IsAssignableFromGenericDefinition(_, typeof(MessageHandler<>), out var baseAssignable);
                    return baseAssignable.BaseType.GetGenericArguments()[0];
                }, _ => _);
        }

        public void ProcessMessage(object message)
        {
            var messageType = message.GetType();
            if (!_MessagesHandlersMap.TryGetValue(messageType, out var handlerType)) {
                Debug.LogWarning($"handler not exists for message of type {messageType}");
                return;
            }

            if (!_HandlersCache.TryGetValue(handlerType, out var handler))
            {
                var provider = new SingletonProvider<MessageHandlerWrapper>();
                handler = (MessageHandlerWrapper)provider.GetObject(_Container, handlerType);
                _HandlersCache.Add(handlerType, handler);
            }
            handler.MessageObjectReceived(message);
        }
        
        private static bool IsAssignableFromGenericDefinition(Type type, Type genericDefinition, out Type baseAssignable) {
            var resultType = type;
            while (resultType.BaseType != null && resultType.BaseType != typeof(object)) {
                if (resultType.BaseType.IsGenericType && resultType.BaseType.GetGenericTypeDefinition() == genericDefinition) {
                    baseAssignable = resultType;
                    return true;
                }
                resultType = resultType.BaseType;
            }
            baseAssignable = null;
            return false;
        }
    }
}