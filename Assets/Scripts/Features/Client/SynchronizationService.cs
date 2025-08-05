using System;
using System.Collections.Generic;
using System.Reflection;
using DependencyInjection;
using Server;
using Shared;
using Steamworks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Client
{
    public class SynchronizationService : ILoadableService, IUnloadableService
    {
        private readonly IMessageSender _MessageSender;
        private readonly IServerProvider _ServerProvider;
        private readonly Container _Container;
        
        private Dictionary<Type, object> _ReceiversCache = new Dictionary<Type, object>();
        private FuncCacheProvider<Type, MethodInfo> _ReceiverMethodsCache;
        private FuncCacheProvider<Type, Type> _ReceiverTypesCache;
        private object[] _ParamsBuffer = new object[2];

        public SynchronizationService(IMessageSender messageSender, IServerProvider serverProvider, Container container)
        {
            _MessageSender = messageSender;
            _ServerProvider = serverProvider;
            _Container = container;
            
            _ReceiverTypesCache = new FuncCacheProvider<Type, Type>(_ => typeof(ISnapshotDataUpdateReceiver<>).MakeGenericType(_));
            _ReceiverMethodsCache = new FuncCacheProvider<Type, MethodInfo>(_ =>
                _.GetMethod("ReceiveSnapshotDataUpdate", BindingFlags.Public | BindingFlags.Instance));
        }
        
        void ILoadableService.Load() { }

        public void ReceiveSnapshot(GameSnapshot snapshot, long serverTimestamp)
        {
            //TODO: send message about receiving ??
            if (snapshot == null)
                return;

            _ServerProvider.SetCurrentTick(serverTimestamp);
            foreach (var snapshotField in typeof(GameSnapshot).GetFields()) {
                var receiverType = _ReceiverTypesCache.Get(snapshotField.FieldType);
                if (!_ReceiversCache.TryGetValue(receiverType, out var receiver)) {
                    receiver = _Container.Resolve(receiverType);
                    if (receiver != null)
                        _ReceiversCache.Add(receiverType, receiver);
                }
                if (receiver == null)
                    continue;
                var updateMethod = _ReceiverMethodsCache.Get(receiverType);
                _ParamsBuffer[0] = serverTimestamp;
                updateMethod.Invoke(receiver, _ParamsBuffer);
            }
        }

        void IUnloadableService.Unload()
        {
            
        }
    }
}