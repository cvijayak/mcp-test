namespace CMS.Mcp.Shared.Api.Clients.Contracts.Configs
{
    public class ClientCredentialsConfig
    {
        public string ClientId { get; init; }
        public string ClientSecret { get; init; }
        public int ResponseCacheTimeout { get; init; }
    }
}