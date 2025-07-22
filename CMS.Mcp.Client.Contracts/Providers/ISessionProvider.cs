namespace CMS.Mcp.Client.Contracts.Providers 
{
    using System;
    using System.Threading.Tasks;

    public interface ISessionProvider
    {
        Task<string> GetAccessTokenAsync();
        Task<DateTimeOffset?> GetExpiresAtUtcAsync();
    }
}
