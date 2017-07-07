using System;
using System.Runtime.Caching;

namespace TripLine.Toolbox
{
    public class AutoRenewMemoryCache<T> where T : class
    {
        private readonly Func<string, T> _getData;
        private readonly TimeSpan _lifeTime;
        private MemoryCache _cache;
        private Func<CacheItemPolicy> _cachePolicyBuilder;

        public AutoRenewMemoryCache(string uniqueName, Func<string, T> getData, TimeSpan? lifeTime = null)
        {
            _getData = getData;
            _cache = new MemoryCache(uniqueName);
            _cachePolicyBuilder = () => new CacheItemPolicy();

            if (lifeTime.HasValue)
            {
                _lifeTime = lifeTime.Value;
                _cachePolicyBuilder = () => new CacheItemPolicy() {AbsoluteExpiration = DateTime.Now + _lifeTime};
            }
        }

        public T GetFromCache(string key)
        {
            var value = _cache.Get(key);

            if (value == null)
            {
                // Getting value from the func
                value = _getData(key);
                SetInCache(key, (T)value);
            }

            return (T)value;
        }

        public void SetInCache(string key, T value)
        {
            _cache.Set(key, value, _cachePolicyBuilder());
        }

        public Maybe<T> RemoveFromCache(string key)
        {
            return new Maybe<T>((T)_cache.Remove(key));
        }
    }
}