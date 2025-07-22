namespace CMS.Mcp.Shared.Api.Clients.Caches
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Collections;
    using Contracts.Caches;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Logging;

    public class ClientResponseCache(IMemoryCache memoryCache, ILogger<ClientResponseCache> logger) : IClientResponseCache
    {
        internal class ResponseCacheKey
        {
            private readonly string _className;
            private readonly string _methodName;
            private readonly string _prefix;
            private readonly IDictionary<string, object> _methodArgs;
            private string _key;

            private ResponseCacheKey(string prefix, string className, string methodName, IDictionary<string, object> methodArgs)
            {
                _className = className;
                _methodName = methodName;
                _methodArgs = methodArgs;
                _prefix = prefix;
            }

            private string Key
            {
                get
                {
                    string GetMethodArgArray() => string.Join(",", _methodArgs?.Select(s => $"{s.Key}:{s.Value}") ?? []);
                    _key ??= $"{_prefix}[{_className}].{_methodName}({GetMethodArgArray()})";

                    return _key;
                }
            }

            public override bool Equals(object obj) =>
                obj is ResponseCacheKey key && Key == key.ToString();

            public override int GetHashCode() => Key.GetHashCode();

            public override string ToString() => Key;

            public static ResponseCacheKey Create<T>(string prefix, string methodName, Dictionary<string, object> methodArgs)
            {
                var methodArgMap = methodArgs?.ToDictionary(x => x.Key, x => x.Value);
                return new ResponseCacheKey(prefix, typeof(T).Name, methodName, methodArgMap);
            }
        }

        private readonly ConcurrentHashSet<ResponseCacheKey> _cacheKeys = new();

        public async Task<TResponse> ReadCacheAsync<TApiClient, TResponse>(Func<ICacheEntry, Task<TResponse>> factory, string prefix, string methodName = null, Dictionary<string, object> methodArgs = null)
        {
            var cacheKey = ResponseCacheKey.Create<TApiClient>(prefix, methodName, methodArgs);
            var cache = await memoryCache.GetOrCreateAsync(cacheKey, f =>
            {
                var c = factory(f);
                logger.LogInformation($"Refreshed cache, CacheKey : {cacheKey}");

                return c;
            });
            _cacheKeys.Add(cacheKey);

            logger.LogDebug($"Read from cache, CacheKey : {cacheKey}");

            return cache;
        }

        public void ClearCache()
        {
            foreach (var cacheKey in _cacheKeys)
            {
                memoryCache.Remove(cacheKey);
                logger.LogInformation($"Removed cache, CacheKey : {cacheKey}");
            }
        }
    }
}