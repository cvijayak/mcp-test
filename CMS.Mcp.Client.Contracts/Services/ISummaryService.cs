namespace CMS.Mcp.Client.Contracts.Services
{
    using System.Threading.Tasks;
    using Microsoft.SemanticKernel;

    public interface ISummaryService
    {
        Task SummarizeAsync(Kernel kernel);
        string Get();
    }
}
