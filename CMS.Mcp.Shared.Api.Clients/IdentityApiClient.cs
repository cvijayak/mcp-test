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

    public class IdentityApiClient(IdentityClientParams clientParams,
        Func<RestClientParams, IRestClient> clientFactory,
        IClientResponseCache clientResponseCache,
        IJsonObjectSerializer serializer) : ClientBase(clientParams.RestClientParams, clientFactory), IIdentityApiClient
    {
        protected async Task<TResponse> ReadCacheAsync<TResponse>(Func<Task<(TResponse, int)>> predicate, [CallerMemberName] string methodName = null, Dictionary<string, object> methodArgs = null)
        {
            return await clientResponseCache.ReadCacheAsync<IIdentityApiClient, TResponse>(async entry =>
            {
                var (response, cacheTimeout) = await predicate();
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(cacheTimeout);
                return response;
            }, GetType().FullName, methodName, methodArgs);
        }

        private async Task<T> GetAccessTokenAsync<T>(IReadOnlyDictionary<string, string> content, CancellationToken cancellationToken) where T : IApiResponse
        {
            var clientId = clientParams.ClientCredentialsConfig.ClientId;
            var clientSecret = clientParams.ClientCredentialsConfig.ClientSecret;

            var restClient = CreateRestClient();
            var configResponse = await GetConfigurationAsync(cancellationToken);

            var tokenEndpointRelativeUri = new Uri(configResponse.TokenEndpoint.PathAndQuery, UriKind.Relative);
            return await restClient.PostAsync(new PostFormApiRequest<T>
            {
                Url = tokenEndpointRelativeUri,
                Content = content,
                AuthenticationHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"))),
                AcceptHeader = new MediaTypeWithQualityHeaderValue("application/json"),
                Deserialize = serializer.Deserialize<T>
            }, cancellationToken);
        }

        public async Task<OidcConfigResponse> GetConfigurationAsync(CancellationToken cancellationToken) 
        {
            var cacheKey = new Dictionary<string, object>
            {
                [nameof(RestClientParams.ExternalServerType)] = clientParams.RestClientParams.ExternalServerType,
                [nameof(RestClientParams.HttpClientInstance)] = clientParams.RestClientParams.HttpClientInstance,
                [nameof(OidcEndpointConfig.GetWellKnownConfigUri)] = clientParams.OidcEndpointConfig.GetWellKnownConfigUri()
            };

            return await ReadCacheAsync(async () =>
            {
                var restClient = CreateRestClient();
                var response = await restClient.GetAsync(new GetApiRequest<OidcConfigResponse>
                {
                    Url = clientParams.OidcEndpointConfig.GetWellKnownConfigUri(),
                    Deserialize = serializer.Deserialize<OidcConfigResponse>
                }, cancellationToken);

                return (response, clientParams.OidcEndpointConfig.ResponseCacheTimeout);
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