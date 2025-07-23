namespace CMS.Mcp.Client.Contracts.Services
{
    using System.Collections.Generic;
    using System.Text.Json.Nodes;
    using System.Threading.Tasks;
    using Models;

    public interface IMcpToolService
    {
        Task<McpToolViewModel[]> GetToolsAsync();
        Task<JsonNode> ExecuteToolAsync(string toolName, Dictionary<string, object> parameters);
        Task RegisterToolsAsync();
    }
}