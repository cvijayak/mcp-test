namespace CMS.Mcp.Shared.Api.Clients.Contracts.Configs
{
    using System;

    public class IdentityClientConfig
    {
        public Uri BaseUrl { get; init; }
        public OidcEndpointConfig OidcEndpoint { get; init; }
        public ClientCredentialsConfig ClientCredentials { get; init; }
        public string Scope { get; init; }
    }
}