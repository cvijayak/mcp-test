namespace CMS.Mcp.Client.Contracts.Services
{
    using System.Threading;
    using System.Threading.Tasks;
    using Models;

    public interface IAiAssistantService
    {
        Task<ChatMessageViewModel> SendMessageAsync(string message, string serverName, CancellationToken cancellationToken);
        Task<string[]> GetSuggestionsAsync(string serverName, CancellationToken cancellationToken);
        void ClearChat();
    }
}
