namespace CMS.Mcp.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.Options;
    using Microsoft.Extensions.Options;
    using Microsoft.SemanticKernel;
    using Microsoft.SemanticKernel.Connectors.OpenAI;
    using ModelContextProtocol.Client;
    using ModelContextProtocol.Protocol;

    public class McpClientProxy(Func<Uri, string, Task<IMcpClient>> mcpClient, IOptions<ServerOptions> serverOptions, Kernel kernel) : IMcpClientProxy
    {
        private readonly ServerOptions _serverOptions = serverOptions.Value;

        private async Task<IMcpClient> GetMcpClient() => await mcpClient(_serverOptions.GetSseUri(), "MonkeyMcpClientTool");

        public async Task<IList<McpClientTool>> ListToolsAsync()
        {
            var client = await GetMcpClient();
            return await client.ListToolsAsync();
        }

        public async Task<CallToolResult> CallToolAsync(string toolName, Dictionary<string, object> parameters) 
        {
            var client = await GetMcpClient();
            return await client.CallToolAsync(toolName, parameters);
        }

        public async Task<FunctionResult> InvokePromptAsync(string message)
        {
#pragma warning disable SKEXP0001
            var client = await GetMcpClient();
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