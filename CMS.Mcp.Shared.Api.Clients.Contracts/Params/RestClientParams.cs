namespace CMS.Mcp.Shared.Api.Clients.Contracts.Params
{
    using System;
    using System.Net.Http;

    public class RestClientParams
    {
        public string HttpClientInstance { get; init; }
        public string ExternalServerType { get; init; }

        public Action<HttpClient> ConfigureHttpClient { get; init; }
    }
}