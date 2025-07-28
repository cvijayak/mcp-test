namespace CMS.Mcp.Client.Contracts
{
    public interface ISummaryStore
    {
        string Get();
        string Add(string summary);
    }
}