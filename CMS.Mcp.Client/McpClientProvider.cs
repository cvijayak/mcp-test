namespace CMS.Mcp.Client 
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.Options;
    using Contracts.Providers;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using ModelContextProtocol.Client;

    public class McpClientProvider : IMcpClientProvider
    {
        private readonly Lazy<Task<IMcpClient>> _clientTask;
        private readonly IServiceProvider _serviceProvider;

        public McpClientProvider(IServiceProvider serviceProvider) 
        {
            _serviceProvider = serviceProvider;
            _clientTask = new Lazy<Task<IMcpClient>>(async () =>
            {
                var serverOptions = _serviceProvider.GetRequiredService<IOptions<ServerOptions>>();
                var sessionProvider = _serviceProvider.GetRequiredService<ISessionProvider>();
                
                var endpoint = serverOptions.Value.GetSseUri();
                var authInjectingTransport = new McpSseTransport(endpoint, "TrimbleCMS", sessionProvider);

                return await McpClientFactory.CreateAsync(authInjectingTransport);
            });
        }

        //private async Task<bool> TryRefreshTokenAsync()
        //{
        //    var httpClient = _serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient();
        //    var response = await httpClient.PostAsync("/mcp/account/refresh-token", null);

        //    return response.IsSuccessStatusCode;
        //}

        public async Task<IMcpClient> GetClientAsync() => await _clientTask.Value;

        //public async Task<TResult> GetClientAsync<TResult>(Func<IMcpClient, ValueTask<TResult>> call)
        //{
        //    var client = await GetClientAsync();
        //    try
        //    {
        //        return await call(client);
        //    }
        //    catch (Exception ex) when (ex.Message.Contains("401") || ex.Message.Contains("Unauthorized"))
        //    {
        //        if (!await TryRefreshTokenAsync())
        //        {
        //            throw;
        //        }

        //        client = await GetClientAsync();
        //        return await call(client);
        //    }
        //}
    }
}