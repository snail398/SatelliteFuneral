using System;

namespace Server
{
    public class FuncCacheProvider<TKey, TValue>: CacheProvider<TKey, TValue> {
        private readonly Func<TKey, TValue> _FuncProvider;

        protected override TValue ExtractFromCache(TKey key) {
            return _FuncProvider.Invoke(key);
        }

        public FuncCacheProvider(Func<TKey, TValue> funcProvider) {
            _FuncProvider = funcProvider;
        }
    }
}