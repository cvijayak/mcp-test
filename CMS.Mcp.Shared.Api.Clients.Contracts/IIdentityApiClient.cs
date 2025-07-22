namespace CMS.Mcp.Shared.Api.Clients.Contracts
{
    using System.Threading;
    using System.Threading.Tasks;
    using Responses;

    public interface IIdentityApiClient : IApiClient
    {
        Task<OidcConfigResponse> GetConfigurationAsync(CancellationToken cancellationToken);
        Task<RefreshTokenResponse> RefreshAccessTokenAsync(string refreshToken, CancellationToken cancellationToken);
    }
}