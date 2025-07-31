namespace CMS.Mcp.Client.Contracts.Services
{
    using System.Threading;
    using System.Threading.Tasks;
    using Models;

    public interface IAssistantService
    {
        Task<ChatMessageViewModel> SendMessageAsync(string message, string serverName, CancellationToken cancellationToken);
        void ClearMessages();
    }
}
