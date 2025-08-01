﻿using System;

namespace Utils
{
    internal class SignalSubscription<T> : SignalSubscriptionWrapper {
        public readonly Action<T> Callback;
        public override object Identifier { get; }
        
        public SignalSubscription(Action<T> callback, object identifier) {
            Callback = callback;
            Identifier = identifier;
        }
    }
}