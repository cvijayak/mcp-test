namespace CMS.Mcp.Client.Contracts
{
    using System;
    using System.Threading.Tasks;
    using ModelContextProtocol.Client;

    public interface IMcpClientProvider
    {
        Task<IMcpClient> GetClientAsync();
        //Task<TResult> GetClientAsync<TResult>(Func<IMcpClient, ValueTask<TResult>> call);
    }
}