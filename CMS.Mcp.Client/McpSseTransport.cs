﻿namespace CMS.Mcp.Client
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using ModelContextProtocol.Client;
    using ModelContextProtocol.Protocol;
    using Security.Contracts.Providers;

    public class McpSseTransport(Uri endpoint, string name, ISessionProvider sessionProvider) : IClientTransport
    {
        public async Task<ITransport> ConnectAsync(CancellationToken cancellationToken) 
        {
            var token = await sessionProvider.GetAccessTokenAsync();

            var options = new SseClientTransportOptions 
            {
                Endpoint = endpoint,
                Name = name,
                ConnectionTimeout = TimeSpan.FromSeconds(60),
                AdditionalHeaders = new Dictionary<string, string>
                {
                    { "Authorization", $"Bearer {token}" }
                }
            };

            var sseTransport = new SseClientTransport(options);
            return await sseTransport.ConnectAsync(cancellationToken);
        }

        public string Name => name;
    }
}
