namespace CMS.Mcp.Shared.Api.Clients.Contracts.Requests 
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;

    public class ApiRequestBase
    {
        private readonly HttpMethod _httpMethod;

        protected ApiRequestBase(HttpMethod httpMethod)
        {
            _httpMethod = httpMethod;
        }

        public Uri Url { get; init; }
		public IReadOnlyDictionary<string, string> QueryParams { get; init; }
		public AuthenticationHeaderValue AuthenticationHeader { get; init; }
		public MediaTypeWithQualityHeaderValue AcceptHeader { get; init; }

        protected virtual void AddHeaders(HttpRequestHeaders headers)
        {
            headers.Authorization = AuthenticationHeader ?? headers.Authorization;

            if (AcceptHeader != null) 
            {
                headers.Accept.Add(AcceptHeader);
            }
        }

        protected virtual string GetQueryString() => 
            QueryParams?.Any() == true ? $"?{string.Join("&", QueryParams.Select(kvp => $"{kvp.Key}={kvp.Value}"))}" : string.Empty;

        protected virtual HttpRequestMessage CreateHttpRequestMessage(string queryString) 
        {
            var requestUri = new Uri($"{Url}{queryString}", UriKind.Relative);

            var httpRequestMessage = new HttpRequestMessage(_httpMethod, requestUri);
            AddHeaders(httpRequestMessage.Headers);

            return httpRequestMessage;
        }

        public virtual HttpRequestMessage CreateHttpRequestMessage()
        {
            var queryString = GetQueryString();
            return CreateHttpRequestMessage(queryString);
        }
    }
}