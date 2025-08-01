using System;

namespace Shared
{
    public interface ITimerProvider
    {
        object CreateTimer(Action callback, int intervalMs, int dueTimeMs);
        object ScheduledAction(Action callback, int dueTimeMs);
        void StopTimer(ref object timer);
    }
}