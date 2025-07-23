namespace CMS.Mcp.Client.Security.Contracts.Providers 
{
    using System;
    using System.Threading.Tasks;

    public interface ISessionProvider
    {
        Task<string> GetAccessTokenAsync();
        Task<DateTimeOffset?> GetExpiresAtUtcAsync();
    }
}
