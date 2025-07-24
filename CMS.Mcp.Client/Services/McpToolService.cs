namespace CMS.Mcp.Client.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using System.Threading.Tasks;
    using Contracts.Models;
    using Contracts.Options;
    using Contracts.Services;
    using Microsoft.Extensions.Options;
    using Microsoft.SemanticKernel;
    using ModelContextProtocol.Client;
    using NJsonSchema;
    using Shared.Common.Extensions;

    public class McpToolService(string serverName, Func<McpServerConfig, Task<IMcpClient>> mcpClientFactory, Func<string, Kernel> kernelFactory, IOptions<ServerOptions> serverOptions) : IMcpToolService
    {
        private readonly ServerOptions _serverOptions = serverOptions.Value;

        private async Task<IMcpClient> GetMcpClient()
        {
            var mcpServer = _serverOptions.McpServers?.FirstOrDefault(d => d.Name == serverName);
            return mcpServer == null ? null : await mcpClientFactory(mcpServer);
        }

        public async Task<McpToolViewModel[]> GetToolsAsync()
        {
            var client = await GetMcpClient();
            if (client == null)
            {
                return [];
            }

            var result = await client.ListToolsAsync();
            return await Task.WhenAll(result.Select(async d =>
            {
                var schema = await JsonSchema.FromJsonAsync(JsonSerializer.Serialize(d.ProtocolTool.InputSchema));
                var properties = schema.Properties.Select(p => new McpToolViewModel.McpToolParameter
                {
                    Name = p.Value.Name,
                    Description = p.Value.Description,
                    Type = p.Value.Type.ToString()
                }).ToArray();

                return new McpToolViewModel
                {
                    Title = d.Title,
                    Name = d.Name,
                    Description = d.Description,
                    Parameters = properties
                };
            }));
        }

        public async Task<JsonNode> ExecuteToolAsync(string toolName, Dictionary<string, object> parameters)
        {
            var client = await GetMcpClient();
            if (client == null)
            {
                return null;
            }

            var result = await client.CallToolAsync(toolName, parameters);
            var textResult = (string)((dynamic)result.Content.FirstOrDefault())?.Text ?? string.Empty;
            var textResultIsValidJson = textResult.IsValidJson();
            var text = result.IsError == true ? JsonSerializer.Serialize(new { error = textResult })
                : (textResultIsValidJson ? textResult : JsonSerializer.Serialize(new { message = textResult }));

            return JsonNode.Parse(text);
        }

        public async Task RegisterToolsAsync()
        {
            var client = await GetMcpClient();
            if (client == null)
            {
                return;
            } 

            var kernel = kernelFactory(serverName);
            if (kernel != null && !kernel.Plugins.Contains(serverName))
            {
                var tools = await client.ListToolsAsync();
#pragma warning disable SKEXP0001
                kernel.Plugins.AddFromFunctions(serverName, tools.Select(t => t.AsKernelFunction()));
#pragma warning restore SKEXP0001
            }
        }
    }
}