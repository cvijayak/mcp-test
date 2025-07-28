namespace CMS.Mcp.Client
{
    using System;
    using System.Collections.Generic;
    using Contracts;
    using Microsoft.Extensions.DependencyInjection;
    using Security.Contracts.Providers;

    public class SummaryStore(IServiceProvider serviceProvider) : ISummaryStore
    {
        private readonly Dictionary<Guid, string> _summaries = [];

        private Guid GetUserId()
        {
            var claimStoreProvider = serviceProvider.GetRequiredService<IClaimStoreProvider>();
            return claimStoreProvider.UserId.GetValueOrDefault();
        }

        public string Get()
        {
            var userId = GetUserId();
            return !_summaries.ContainsKey(userId) ? string.Empty : _summaries[userId];
        }

        public string Add(string summary)
        {
            var userId = GetUserId();
            _summaries[userId] = summary;

            return summary;
        }
    }
}