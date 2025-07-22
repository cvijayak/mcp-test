namespace CMS.Mcp.Shared.Api.Clients.Contracts
{
    using System.Threading;
    using System.Threading.Tasks;
    using Requests;

    public interface IRestClient
    {
        Task<TResponse> PostAsync<TResponse>(PostFormApiRequest<TResponse> request, CancellationToken cancellationToken = default) where TResponse : IApiResponse;
        Task<TResponse> GetAsync<TResponse>(GetApiRequest<TResponse> request, CancellationToken cancellationToken = default) where TResponse : IApiResponse;
    }
}