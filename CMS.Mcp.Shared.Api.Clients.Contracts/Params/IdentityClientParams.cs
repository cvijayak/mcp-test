namespace CMS.Mcp.Shared.Api.Clients.Contracts.Params
{
    using Configs;

    public class IdentityClientParams 
    {
        public RestClientParams RestClientParams { get; init; }
        public OidcEndpointConfig OidcEndpointConfig { get; init; }
        public ClientCredentialsConfig ClientCredentialsConfig { get; init; }
        public string Scope { get; init; }
    }
}