namespace CMS.Mcp.Client 
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.Options;
    using Contracts.Providers;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.SemanticKernel;
    using Microsoft.SemanticKernel.Connectors.OpenAI;
    using ModelContextProtocol.Client;
    using ModelContextProtocol.Protocol;

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

                var mcpClient = await McpClientFactory.CreateAsync(authInjectingTransport);
                var kernel = _serviceProvider.GetRequiredService<Kernel>();

                var tools = await mcpClient.ListToolsAsync();

#pragma warning disable SKEXP0001
                kernel.Plugins.AddFromFunctions("MonkeyMcpClientTool", tools.Select(t => t.AsKernelFunction()));
#pragma warning restore SKEXP0001

                return mcpClient;
            });
        }

        public async Task<IList<McpClientTool>> ListToolsAsync()
        {
            var client = await _clientTask.Value;
            return await client.ListToolsAsync();
        }

        public async Task<CallToolResult> CallToolAsync(string toolName, Dictionary<string, object> parameters) 
        {
            var client = await _clientTask.Value;
            return await client.CallToolAsync(toolName, parameters);
        }

        public async Task<FunctionResult> InvokePromptAsync(string message)
        {
            var client = await _clientTask.Value;

#pragma warning disable SKEXP0001
            var execSettings = new OpenAIPromptExecutionSettings 
            {
                Temperature = 0,
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new() { RetainArgumentTypes = true })
            };

            var kernel = _serviceProvider.GetRequiredService<Kernel>();
            return await kernel.InvokePromptAsync(message, new KernelArguments(execSettings));
#pragma warning restore SKEXP0001
        }
    }
}