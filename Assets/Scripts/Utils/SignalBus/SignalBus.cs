using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Unity.IL2CPP.CompilerServices;

namespace Utils {
    public sealed class SignalBus {
        private readonly Dictionary<Type, OrderedDictionary> _SubscriptionsMap = new Dictionary<Type, OrderedDictionary>();
        private readonly Dictionary<Type, FiredSignalState> _CurrentFiredSignals = new Dictionary<Type, FiredSignalState>();

        public void Subscribe<TSignal>(Action<TSignal> callback, object identifier) {
            RunOrSchedule(typeof(TSignal), () => {
                if (!_SubscriptionsMap.TryGetValue(typeof(TSignal), out var subscriptions)) {
                    subscriptions = new OrderedDictionary();
                    _SubscriptionsMap.Add(typeof(TSignal), subscriptions);
                }
                subscriptions.Add(identifier, new SignalSubscription<TSignal>(callback, identifier));
            });
        }

        public void UnSubscribe<TSignal>(object identifier) {
            RunOrSchedule(typeof(TSignal), () => {
                if (!_SubscriptionsMap.TryGetValue(typeof(TSignal), out var subscriptions))
                    return;
                subscriptions.Remove(identifier);
            });
        }

        public void UnSubscribeFromAll(object identifier) {
            foreach (var subscriptions in _SubscriptionsMap) {
                RunOrSchedule(subscriptions.Key, () => {
                    subscriptions.Value.Remove(identifier);
                });
            }
        }
        
        /// avoid circular fire (firing signal from it's listener)
        public void FireSignal<TSignal>(TSignal signal) {
            if (!_CurrentFiredSignals.TryGetValue(typeof(TSignal), out var lastState)) {
                _CurrentFiredSignals.Add(typeof(TSignal), new FiredSignalState(null, 1));
            }
            else {
                _CurrentFiredSignals[typeof(TSignal)] = new FiredSignalState(lastState.OnComplete, lastState.CurrentFireCount + 1);
            }
            var subscriptions = GetSignalSubscriptions<TSignal>();
#if SAFE_SIGNAL_BUS
            try {  
#endif
                if (subscriptions != null) {
                    var subscriptionsCount = subscriptions.Count;
                    for (var i = 0; i < subscriptionsCount; i++) {
                        ((SignalSubscription<TSignal>)subscriptions[i]).Callback.Invoke(signal);
                    }
                }
#if SAFE_SIGNAL_BUS
            }
            finally {
#endif
                lastState = _CurrentFiredSignals[typeof(TSignal)];
                lastState.OnComplete?.Invoke();
                if (lastState.CurrentFireCount - 1 > 0) {
                    _CurrentFiredSignals[typeof(TSignal)] = new FiredSignalState(null, lastState.CurrentFireCount - 1);
                }
                else {
                    _CurrentFiredSignals.Remove(typeof(TSignal));
                }
#if SAFE_SIGNAL_BUS
            }
#endif
        }

        private OrderedDictionary GetSignalSubscriptions<TSignal>() {
            if (!_SubscriptionsMap.TryGetValue(typeof(TSignal), out var subscriptions))
                return null;
            return subscriptions;
        }

        private void RunOrSchedule(Type signalType, Action operation) {
            if (_CurrentFiredSignals.TryGetValue(signalType, out var firedSignalState)) {
                _CurrentFiredSignals[signalType] = new FiredSignalState(firedSignalState.OnComplete + operation, firedSignalState.CurrentFireCount);
            }
            else {
                operation.Invoke();
            }
        }
    }
}