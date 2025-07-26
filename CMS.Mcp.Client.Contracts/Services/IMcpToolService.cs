namespace CMS.Mcp.Client.Contracts.Services
{
    using System.Collections.Generic;
    using System.Text.Json.Nodes;
    using System.Threading;
    using System.Threading.Tasks;
    using Models;

    public interface IMcpToolService
    {
        Task<McpToolViewModel[]> GetToolsAsync(CancellationToken cancellationToken);
        Task<JsonNode> ExecuteToolAsync(string toolName, Dictionary<string, object> parameters, CancellationToken cancellationToken);
        Task RegisterToolsAsync(CancellationToken cancellationToken);
    }
}