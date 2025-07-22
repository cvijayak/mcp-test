namespace CMS.Mcp.Shared.Api.Clients.Contracts.Configs
{
    using System;

    public class OidcEndpointConfig
    {
        private const string GET_WELL_KNOWN_CONFIG_URL = "/.well-known/openid-configuration";

        public int ResponseCacheTimeout { get; init; }

        public Uri GetWellKnownConfigUri() => new(GET_WELL_KNOWN_CONFIG_URL, UriKind.Relative);
    }
}