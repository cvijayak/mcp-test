namespace CMS.Mcp.Client.Contracts.Options
{
    using System;

    public class McpServerConfig
    {
        public string Name { get; set; }
        public string BaseUrl { get; set; }

        public Uri GetSseUri() => new($"{BaseUrl.TrimEnd('/')}/sse");
    }

    public class ServerOptions
    {
        public McpServerConfig[] McpServers { get; set; } = [];
    }
}