namespace CMS.Mcp.Shared.Api.Clients 
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.Requests;
    using Common.Extensions;
    using Contracts.Params;
    using Extensions;
    using Microsoft.Extensions.Logging;

    public class RestClient(RestClientParams restClientParams, IHttpClientFactory factory, ILogger<RestClient> logger) : IRestClient
    {
        private readonly string _serverType = restClientParams.ExternalServerType;
        private readonly string _httpClientInstance = restClientParams.HttpClientInstance;

        private HttpClient CreateHttpClient()
        {
            var httpClient = !string.IsNullOrEmpty(_httpClientInstance) ? factory.CreateClient(_httpClientInstance) : factory.CreateClient();
            restClientParams.ConfigureHttpClient(httpClient);

            return httpClient;
        }

        public async Task<TResponse> PostAsync<TResponse>(PostFormApiRequest<TResponse> request, CancellationToken cancellationToken = default) 
            where TResponse : IApiResponse
        {
            var httpClient = CreateHttpClient();
            
            using var httpRequestMessage = request.CreateHttpRequestMessage();
            using var response = await httpClient.SendAsync(_serverType, httpRequestMessage, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var exception = await response.BuildAsync(_serverType);
                var resourceName = typeof(TResponse).Name.ToCamelCase().Replace("Response", string.Empty);
                logger.LogError(exception, $"Exception occurred while getting {resourceName}");

                throw exception;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return request.Deserialize(stream);
        }

        public async Task<TResponse> GetAsync<TResponse>(GetApiRequest<TResponse> request, CancellationToken cancellationToken = default) 
            where TResponse : IApiResponse
        {
            var httpClient = CreateHttpClient();
            
            using var httpRequestMessage = request.CreateHttpRequestMessage();
            using var response = await httpClient.SendAsync(_serverType, httpRequestMessage, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var exception = await response.BuildAsync(_serverType);
                var resourceName = typeof(TResponse).Name.ToCamelCase().Replace("Response", string.Empty);
                logger.LogError(exception, $"Exception occurred while getting {resourceName}");

                throw exception;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return request.Deserialize(stream);
        }
    }
}