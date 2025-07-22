namespace CMS.Mcp.Client.Contracts.Services
{
    using System.Collections.Generic;
    using System.Text.Json.Nodes;
    using System.Threading.Tasks;
    using Models;

    public interface IChatService
    {
        List<ChatMessageViewModel> Messages { get; }
        
        Task<string[]> GetToolsAsync();
        Task<JsonNode> ExecuteToolAsync(string toolName, Dictionary<string, object> parameters);
        Task<ChatMessageViewModel> SendMessageAsync(string message);
        Task ClearChatAsync();
    }
}
