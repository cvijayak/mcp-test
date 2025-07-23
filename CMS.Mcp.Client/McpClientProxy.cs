namespace CMS.Mcp.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.Options;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.SemanticKernel;
    using Microsoft.SemanticKernel.Connectors.OpenAI;
    using ModelContextProtocol.Client;
    using ModelContextProtocol.Protocol;
    using Security.Contracts.Providers;

    public class McpClientProxy : IMcpClientProxy
    {
        private readonly Lazy<Task<IMcpClient>> _clientTask;
        private readonly IServiceProvider _serviceProvider;

        public McpClientProxy(IServiceProvider serviceProvider) 
        {
            _serviceProvider = serviceProvider;
            _clientTask = new Lazy<Task<IMcpClient>>(async () =>
            {
                var serverOptions = _serviceProvider.GetRequiredService<IOptions<ServerOptions>>();
                var sessionProvider = _serviceProvider.GetRequiredService<ISessionProvider>();
                
                var endpoint = serverOptions.Value.GetSseUri();
                var transport = new McpSseTransport(endpoint, "MonkeyMcpClientTool", sessionProvider);

                return await McpClientFactory.CreateAsync(transport);
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
#pragma warning disable SKEXP0001
            var client = await _clientTask.Value;
            var kernel = _serviceProvider.GetRequiredService<Kernel>();
            if (!kernel.Plugins.Contains("MonkeyMcpClientTool"))
            {
                var tools = await client.ListToolsAsync();
                kernel.Plugins.AddFromFunctions("MonkeyMcpClientTool", tools.Select(t => t.AsKernelFunction()));
            }

            return await kernel.InvokePromptAsync(message, new KernelArguments(new OpenAIPromptExecutionSettings 
            {
                Temperature = 0,
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new() { RetainArgumentTypes = true })
            }));
#pragma warning restore SKEXP0001
        }
    }
}