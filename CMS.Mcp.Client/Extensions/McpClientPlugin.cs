namespace CMS.Mcp.Client.Extensions 
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Contracts;
    using Microsoft.SemanticKernel;
    using ModelContextProtocol.Client;
    using Shared.Common.Extensions;

    public class McpClientTool(IMcpClientProvider mcpClientProvider)
    {
        [KernelFunction, Description("Call a tool on the MCP server")]
        public async Task<string> CallToolAsync
        (
            [Description("The ID of the MCP tool to call")] string toolId, 
            [Description("Input to the tool")] Dictionary<string, object> input
        ) 
        {
            var client = await mcpClientProvider.GetClientAsync();
            var result = await client.CallToolAsync(toolId, input);

            var textResult = (string) ((dynamic) result.Content.FirstOrDefault())?.Text ?? string.Empty;
            var textResultIsValidJson = textResult.IsValidJson();
            return result.IsError == true ? JsonSerializer.Serialize(new { error = textResult })
                : (textResultIsValidJson ? textResult : JsonSerializer.Serialize(new { message = textResult }));
        }
    }
}