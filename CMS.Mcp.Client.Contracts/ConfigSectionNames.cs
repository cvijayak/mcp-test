namespace CMS.Mcp.Client.Contracts
{
    public static class ConfigSectionNames
    {
        public const string CORS_ALLOWED_ORIGINS = "McpClient:CorsAllowedOrigins";
        public const string IDENTITY_SERVER = "McpClient:IdentityServer";
        public const string SERVER = "McpClient:Server";
        public const string AZURE_OPEN_AI_CHAT = "McpClient:AzureOpenAIChat";
        public const string DATA_PROTECTION = "McpClient:DataProtection";
        public const string HEALTH_CHECK_URL = "McpClient:HealthCheckUrl";
    }
}