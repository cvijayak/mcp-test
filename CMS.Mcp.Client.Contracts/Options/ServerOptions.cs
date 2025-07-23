namespace CMS.Mcp.Client.Contracts.Options
{
    using System;

    public class ServerOptions
    {
        public class McpServer
        {
            public string Name { get; set; }
            public string BaseUrl { get; set; }

            public Uri GetSseUri() => new($"{BaseUrl.TrimEnd('/')}/sse");
        }

        public McpServer[] McpServers { get; set; } = [];
    }
}