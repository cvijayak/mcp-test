namespace CMS.Mcp.Client.Contracts.Services
{
    using System.Collections.Generic;
    using System.Text.Json.Nodes;
    using System.Threading;
    using System.Threading.Tasks;
    using Models;

    public interface IMcpToolService
    {
        Task<McpToolViewModel[]> ListAsync(CancellationToken cancellationToken);
        Task<JsonNode> ExecuteAsync(string toolName, Dictionary<string, object> parameters, CancellationToken cancellationToken);
        Task RegisterAsync(CancellationToken cancellationToken);
    }
}