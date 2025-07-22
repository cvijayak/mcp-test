namespace CMS.Mcp.Server.Contracts
{
    using System;

    public class IdentityServerOptions 
    {
        public class JwtBearerConfig 
        {
            public bool RequireHttpsMetadata { get; init; }
            public string[] ValidAudiences { get; init; }
            public string ExpectedScopePrefix { get; init; }
        }

        public Uri Authority { get; init; }
        public JwtBearerConfig JwtBearer { get; init; }
    }
}