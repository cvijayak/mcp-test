namespace CMS.Mcp.Shared.Api.Clients.Contracts.Caches
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Memory;

    public interface IClientResponseCache
    {
        Task<TResponse> ReadCacheAsync<TApiClient, TResponse>(Func<ICacheEntry, Task<TResponse>> factory, string prefix, string methodName = null, Dictionary<string, object> methodArgs = null);
        void ClearCache();
    }
}