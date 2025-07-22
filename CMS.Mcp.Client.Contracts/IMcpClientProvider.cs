namespace CMS.Mcp.Client.Contracts 
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.SemanticKernel;
    using ModelContextProtocol.Client;
    using ModelContextProtocol.Protocol;

    public interface IMcpClientProvider
    {
        Task<IList<McpClientTool>> ListToolsAsync();
        Task<CallToolResult> CallToolAsync(string toolName, Dictionary<string, object> parameters);
        Task<FunctionResult> InvokePromptAsync(string message);
    }
}