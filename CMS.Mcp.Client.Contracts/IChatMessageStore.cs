namespace CMS.Mcp.Client.Contracts
{
    using System.Threading.Tasks;
    using Models;

    public interface IChatMessageStore
    {
        Task AddAsync(ChatMessageViewModel message);
        Task<ChatMessageViewModel[]> ListAsync();
        Task ClearAsync();
    }
}