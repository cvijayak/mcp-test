namespace CMS.Mcp.Client
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using ModelContextProtocol.Client;
    using ModelContextProtocol.Protocol;
    using Shared.Api.Clients.Contracts;

    public class McpSseTransport(Uri endpoint, string name, IHttpClientFactory httpClientFactory) : IClientTransport
    {
        public async Task<ITransport> ConnectAsync(CancellationToken cancellationToken) 
        {
            var httpClientInstanceName = $"{HttpClientInstances.MCP_SERVER_HTTP_CLIENT_INSTANCE}.{name}";
            var httpClient = httpClientFactory.CreateClient(httpClientInstanceName);
            var transportOptions = new SseClientTransportOptions 
            {
                Endpoint = endpoint,
                Name = name,
                ConnectionTimeout = TimeSpan.FromSeconds(60)
            };

            var sseTransport = new SseClientTransport(transportOptions: transportOptions, httpClient: httpClient);
            return await sseTransport.ConnectAsync(cancellationToken);
        }

        public string Name => name;
    }
}
