namespace CMS.Mcp.Client.Contracts.Services
{
    using System.Threading.Tasks;
    using Models;

    public interface IAiAssistantService
    {
        Task<ChatMessageViewModel> SendMessageAsync(string message);
        Task ClearChatAsync();
    }
}
