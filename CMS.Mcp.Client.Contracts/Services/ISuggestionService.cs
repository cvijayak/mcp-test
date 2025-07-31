namespace CMS.Mcp.Client.Contracts.Services
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface ISuggestionService
    {
        Task<string[]> ListAsync(string serverName, CancellationToken cancellationToken);
    }
}