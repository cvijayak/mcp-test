namespace CMS.Mcp.Shared.Api.Clients 
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http.Headers;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Contracts;
    using Contracts;
    using Contracts.Caches;
    using Contracts.Configs;
    using Contracts.Params;
    using Contracts.Requests;
    using Contracts.Responses;

    public class IdentityApiClient : ClientBase, IIdentityApiClient
    {
        private readonly IJsonObjectSerializer _serializer;
        private readonly IdentityClientParams _clientParams;
        private readonly IClientResponseCache _clientResponseCache;

        public IdentityApiClient(IdentityClientParams clientParams, 
            Func<RestClientParams, IRestClient> clientFactory,
            IClientResponseCache clientResponseCache, 
            IJsonObjectSerializer serializer) : base(clientParams.RestClientParams, clientFactory)
        {
            _clientParams = clientParams;
            _clientResponseCache = clientResponseCache;
            _serializer = serializer;
        }

        protected async Task<TResponse> ReadCacheAsync<TResponse>(Func<Task<(TResponse, int)>> predicate, [CallerMemberName] string methodName = null, Dictionary<string, object> methodArgs = null)
        {
            return await _clientResponseCache.ReadCacheAsync<IIdentityApiClient, TResponse>(async entry =>
            {
                var (response, cacheTimeout) = await predicate();
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(cacheTimeout);
                return response;
            }, GetType().FullName, methodName, methodArgs);
        }

        private async Task<T> GetAccessTokenAsync<T>(IReadOnlyDictionary<string, string> content, CancellationToken cancellationToken) where T : IApiResponse
        {
            var clientId = _clientParams.ClientCredentialsConfig.ClientId;
            var clientSecret = _clientParams.ClientCredentialsConfig.ClientSecret;

            var restClient = CreateRestClient();
            var configResponse = await GetConfigurationAsync(cancellationToken);

            var tokenEndpointRelativeUri = new Uri(configResponse.TokenEndpoint.PathAndQuery, UriKind.Relative);
            return await restClient.PostAsync(new PostFormApiRequest<T>
            {
                Url = tokenEndpointRelativeUri,
                Content = content,
                AuthenticationHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"))),
                AcceptHeader = new MediaTypeWithQualityHeaderValue("application/json"),
                Deserialize = _serializer.Deserialize<T>
            }, cancellationToken);
        }

        public async Task<OidcConfigResponse> GetConfigurationAsync(CancellationToken cancellationToken) 
        {
            var cacheKey = new Dictionary<string, object>
            {
                [nameof(RestClientParams.ExternalServerType)] = _clientParams.RestClientParams.ExternalServerType,
                [nameof(RestClientParams.HttpClientInstance)] = _clientParams.RestClientParams.HttpClientInstance,
                [nameof(OidcEndpointConfig.GetWellKnownConfigUri)] = _clientParams.OidcEndpointConfig.GetWellKnownConfigUri()
            };

            return await ReadCacheAsync(async () =>
            {
                var restClient = CreateRestClient();
                var response = await restClient.GetAsync(new GetApiRequest<OidcConfigResponse>
                {
                    Url = _clientParams.OidcEndpointConfig.GetWellKnownConfigUri(),
                    Deserialize = _serializer.Deserialize<OidcConfigResponse>
                }, cancellationToken);

                return (response, _clientParams.OidcEndpointConfig.ResponseCacheTimeout);
            }, GetType().FullName, methodArgs: cacheKey);
        }

        public async Task<RefreshTokenResponse> RefreshAccessTokenAsync(string refreshToken, CancellationToken cancellationToken)
        {
            return await GetAccessTokenAsync<RefreshTokenResponse>(new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "tenantDomain", "trimble.com" },
                { "refresh_token", refreshToken }
            }, cancellationToken);
        }
    }
}