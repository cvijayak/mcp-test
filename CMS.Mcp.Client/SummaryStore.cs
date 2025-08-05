namespace CMS.Mcp.Client
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using Microsoft.Extensions.DependencyInjection;
    using Security.Contracts.Providers;

    public class SummaryStore(IServiceProvider serviceProvider) : ISummaryStore
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly Dictionary<Guid, string> _summaries = [];

        private async Task<T> ExecuteUnderSemaphore<T>(Func<T> action)
        {
            await _semaphore.WaitAsync();
            try
            {
                return action();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task ExecuteUnderSemaphore(Action action)
        {
            await _semaphore.WaitAsync();
            try
            {
                action();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private Guid GetUserId()
        {
            var claimStoreProvider = serviceProvider.GetRequiredService<IClaimStoreProvider>();
            return claimStoreProvider.UserId.GetValueOrDefault();
        }

        public async Task<string> GetAsync()
        {
            return await ExecuteUnderSemaphore(() =>
            {
                var userId = GetUserId();
                return !_summaries.ContainsKey(userId) ? string.Empty : _summaries[userId];
            });
        }

        public async Task<string> AddAsync(string summary)
        {
            return await ExecuteUnderSemaphore(() =>
            {
                var userId = GetUserId();
                _summaries[userId] = summary;

                return summary;
            });
        }

        public async Task ClearAsync()
        {
            await ExecuteUnderSemaphore(() =>
            {
                var userId = GetUserId();
                if (_summaries.ContainsKey(userId))
                {
                    _summaries.Remove(userId);
                }
            });
        }
    }
}