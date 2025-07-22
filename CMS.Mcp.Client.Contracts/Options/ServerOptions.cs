namespace CMS.Mcp.Client.Contracts.Options
{
    using System;

    public class ServerOptions
    {
        public string BaseUrl { get; set; }

        public Uri GetSseUri() => new($"{BaseUrl.TrimEnd('/')}/sse");
    }
}