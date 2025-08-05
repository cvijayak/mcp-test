namespace CMS.Mcp.Client.Contracts
{
    using System.Threading.Tasks;

    public interface ISummaryStore
    {
        Task<string> GetAsync();
        Task<string> AddAsync(string summary);
        Task ClearAsync();
    }
}