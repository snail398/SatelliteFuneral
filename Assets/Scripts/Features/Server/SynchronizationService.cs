using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DependencyInjection;
using Network.Transport;
using Server.Input;
using Shared;
using Steamworks;
using UnityEngine;

namespace Server
{
    public class SynchronizationService
    {
        private const int STORED_SNAPSHOT_AMOUNT = 50;
        
        private readonly ITimerProvider _TimerProvider;
        private readonly Container _Container; 
        private readonly GameServer _GameServer; 
        private readonly MessageBroadcaster _MessageBroadcaster; 
        private readonly InputService _InputService; 
        
        private readonly Dictionary<ulong, int?> _LastReceivedPlayerSnapshot = new Dictionary<ulong, int?>();
        private readonly Dictionary<int, GameSnapshot> _Snapshots = new Dictionary<int, GameSnapshot>();
        protected int _CurrentSnapshotId = -1;
        private object _UpdateTimer;
        
        private static readonly FuncCacheProvider<Type, List<FieldInfo>> _FieldsCacheProvider = new FuncCacheProvider<Type, List<FieldInfo>>(_ => _.GetFields().ToList());
        private static readonly FuncCacheProvider<Type, List<PropertyInfo>> _PropertiesCacheProvider = new FuncCacheProvider<Type, List<PropertyInfo>>(_ => _.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ToList());
        private static readonly FuncCacheProvider<Type, DiffDataAttribute> _TypesDiffDataAttributeCacheProvider = new FuncCacheProvider<Type, DiffDataAttribute>(_ => _.GetCustomAttribute<DiffDataAttribute>());
       
        private static readonly FuncCacheProvider<Type, Func<object>> _ConstructorCacheProvider = new FuncCacheProvider<Type, Func<object>>(type => () => Activator.CreateInstance(type));
        private static readonly FuncCacheProvider<PropertyInfo, Func<object, object>> _PropertyGetterCacheProvider = new FuncCacheProvider<PropertyInfo, Func<object, object>>(propertyInfo => instance => propertyInfo.GetValue(instance));
        private static readonly FuncCacheProvider<FieldInfo, Func<object, object>> _FieldGetterCacheProvider = new FuncCacheProvider<FieldInfo, Func<object, object>>(fieldInfo => instance => fieldInfo.GetValue(instance));
        private static readonly FuncCacheProvider<FieldInfo, Action<object, object>> _FieldSetterCacheProvider = new FuncCacheProvider<FieldInfo, Action<object, object>>(fieldInfo => (instance, argument) => fieldInfo.SetValue(instance, argument));

        
        public SynchronizationService(ITimerProvider timerProvider, Container container, GameServer gameServer, MessageBroadcaster messageBroadcaster, InputService inputService)
        {
            _TimerProvider = timerProvider;
            _Container = container;
            _GameServer = gameServer;
            _MessageBroadcaster = messageBroadcaster;
            _InputService = inputService;
        }

        public void BroadcastSnapshots(uint serverTick)
        {
            Debug.Log("Iterating snapshots");
            SaveCurrentStateToSnapshot(serverTick);
            SendCurrentSnapshotToPlayers();
        }

        private void SaveCurrentStateToSnapshot(uint serverTick)
        {
            var snapshot = new GameSnapshot()
            {
                ServerTick = serverTick,
            };
            
            _FieldsCacheProvider.Get(typeof(GameSnapshot)).ForEach(_ => {
                var providerType = typeof(ISnapshotDataProvider<>).MakeGenericType(_.FieldType);
                var provider = _Container.Resolve(providerType);
                if (provider == null) {
                    Debug.LogWarning($"provider not found for type {providerType}");
                    return;
                }
                var providerProperty = _PropertiesCacheProvider.Get(providerType).First(__ => __.PropertyType == _.FieldType);
                var providerGetter = _PropertyGetterCacheProvider.Get(providerProperty);
                var providerData = providerGetter.Invoke(provider);
                var snapshotSetter = _FieldSetterCacheProvider.Get(_);
                snapshotSetter.Invoke(snapshot, providerData);
            });
            
            _CurrentSnapshotId++;
            _Snapshots.Add(_CurrentSnapshotId, snapshot);
            if (_Snapshots.Count > STORED_SNAPSHOT_AMOUNT) {
                var oldestSnapshot = _CurrentSnapshotId - STORED_SNAPSHOT_AMOUNT;
                _Snapshots.Remove(oldestSnapshot);
            }
        }

        private void SendCurrentSnapshotToPlayers()
        {
            foreach (var player in _GameServer.ConnectedUsers)
            {
                _LastReceivedPlayerSnapshot.TryGetValue(player.GetSteamID().m_SteamID, out var lastReceivedSnapshotId);
                GameSnapshot diff;
                if (!lastReceivedSnapshotId.HasValue || !_Snapshots.ContainsKey(lastReceivedSnapshotId.Value)) {
                    diff = _Snapshots[_CurrentSnapshotId];
                    diff.LastHandledInput = _InputService.GetLastHandledInput(player);
                }
                else
                {
                    diff = GetDiff(_Snapshots[lastReceivedSnapshotId.Value], _Snapshots[_CurrentSnapshotId], out var changed) as GameSnapshot;
                }
                BroadcastDiff(diff, player, lastReceivedSnapshotId);
            }
        }

        private void BroadcastDiff(GameSnapshot diff, SteamNetworkingIdentity playerId, int? lastReceivedSnapshotId)
        {
            var message = new GameSnapshotMessage()
            {
                GameSnapshot = diff,
            };
            _MessageBroadcaster.BroadcastMessage(message, new List<SteamNetworkingIdentity>(){playerId});
        }
        
        public object GetDiff(object old, object current, out bool anythingChanged) {
            anythingChanged = false;
            var type = old != null ? old.GetType() : current?.GetType();
            if (type == null)
                return null;
            if (_TypesDiffDataAttributeCacheProvider.Get(type) == null) {
                if (Equals(old, current))
                    return null;
                anythingChanged = true;
                return current;
            }
            var result = _ConstructorCacheProvider.Get(type).Invoke();
            foreach (var _ in _FieldsCacheProvider.Get(type)) {
                var setter = _FieldSetterCacheProvider.Get(_);
                var getter = _FieldGetterCacheProvider.Get(_);
                var oldValue = getter.Invoke(old);
                var currentValue = getter.Invoke(current);
                var fieldType = _.FieldType;
                var anythingChangedInField = false;
                var diff = typeof(IList).IsAssignableFrom(fieldType)
                    ? GetListDiff(oldValue, currentValue, fieldType)
                    : GetDiff(oldValue, currentValue, out anythingChangedInField);
                if (anythingChangedInField)
                    anythingChanged = true;
                setter.Invoke(result, diff);
            }
            return result;
        }

        /// <summary>
        /// you must not use null lists (null list means that it needs to be restored on client to value from previous snapshot) 
        /// </summary>
        private object GetListDiff(object oldValue, object currentValue, Type listType) {
            var clientList = (IList) oldValue;
            var currentList = (IList) currentValue;
            IList diffList;
            if (clientList == null) {
                diffList = currentList;
            }
            else if (currentList == null || currentList.Count - clientList.Count != 0) {
                diffList = currentList;
            }
            else {
                var diff = new List<object>();
                var anyItemChanged = false;
                for (var index = 0; index < currentList.Count; index++) {
                    var currentItem = currentList[index];
                    var oldItem = clientList[index];
                    bool itemChanged;
                    var itemDiff = GetDiff(oldItem, currentItem, out itemChanged);
                    diff.Add(itemDiff);
                    if (itemChanged)
                        anyItemChanged = true;
                }
                diffList = anyItemChanged ? diff : null;
            }
            if (diffList == null)
                return null;
            var typedList = (IList) _ConstructorCacheProvider.Get(listType).Invoke();
            foreach (var item in diffList)
                typedList.Add(item);
            return typedList;
        }
    }
}