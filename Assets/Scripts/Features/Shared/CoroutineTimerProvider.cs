using System;
using System.Collections;
using UnityEngine;
using Utils;

namespace Shared
{
    public class CoroutineTimerProvider: ITimerProvider {
        private readonly UnityEventProvider _UnityEventProvider;

        public CoroutineTimerProvider(UnityEventProvider unityEventProvider)
        {
            _UnityEventProvider = unityEventProvider;
        }

        public object CreateTimer(Action callback, int intervalMs, int dueTimeMs) {
            return _UnityEventProvider.StartCoroutine(TimerCoroutine(callback, intervalMs, dueTimeMs, true));
        }

        public object ScheduledAction(Action callback, int intervalMs) {
            return _UnityEventProvider.StartCoroutine(TimerCoroutine(callback, intervalMs, 0, false));
        }

        public void StopTimer(ref object timer) {
            if (timer == null)
                return;
            if (!(timer is Coroutine))
                throw new ArgumentException();
            _UnityEventProvider.StopCoroutine((Coroutine) timer);
            timer = null;
        }
        
        private static IEnumerator TimerCoroutine(Action callback, int intervalMs, int dueTimeMs, bool repeat) {
            var startTime = Environment.TickCount;
            while (TimestampExtensions.TimeStampsAbsIncrement(startTime, Environment.TickCount) < dueTimeMs)
                yield return null;
            do {
                startTime = Environment.TickCount;
                while (TimestampExtensions.TimeStampsAbsIncrement(startTime, Environment.TickCount) < intervalMs) 
                    yield return null;
                callback?.Invoke();
                yield return null;
            }
            while (repeat);
        }
    }
}