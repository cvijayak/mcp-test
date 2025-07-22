namespace CMS.Mcp.Client.Contracts.Options
{
    using System;

    public class IdentityServerOptions
    {
        public class AuthorizationCodeConfig
        {
            public string ClientId { get; init; }
            public string ClientSecret { get; init; }
            public string Scopes { get; init; }
            public int ResponseCacheTimeout { get; init; }
        }

        public class OidcEndpointConfig
        {
            private const string GET_WELL_KNOWN_CONFIG_URL = "/.well-known/openid-configuration";

            public int ResponseCacheTimeout { get; init; }

            public Uri GetWellKnownConfigUri() => new(GET_WELL_KNOWN_CONFIG_URL, UriKind.Relative);
        }

        public Uri Authority { get; init; }
        public OidcEndpointConfig OidcEndpoint { get; init; }
        public AuthorizationCodeConfig AuthorizationCode { get; init; }
    }
}