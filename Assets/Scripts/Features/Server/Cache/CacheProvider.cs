using System.Collections.Generic;

namespace Server
{
    public abstract class CacheProvider<TKey, TValue> {
        protected readonly Dictionary<TKey, TValue> _Cache = new Dictionary<TKey, TValue>();
        
        public TValue this[TKey key] => Get(key);

        public virtual TValue Get(TKey key) {
            if (!_Cache.ContainsKey(key)) {
                _Cache.Add(key, ExtractFromCache(key));
            }
            return _Cache[key];
        }

        protected abstract TValue ExtractFromCache(TKey key);
    }
}