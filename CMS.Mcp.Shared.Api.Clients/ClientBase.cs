namespace CMS.Mcp.Shared.Api.Clients
{
    using System;
    using Contracts;
    using Contracts.Params;

    public class ClientBase
    {
        private readonly RestClientParams _restClientParams;
        private readonly Func<RestClientParams, IRestClient> _restClientFactory;

        protected ClientBase(RestClientParams restClientParams, Func<RestClientParams, IRestClient> restClientFactory)
        {
            _restClientParams = restClientParams;
            _restClientFactory = restClientFactory;
        }

        protected IRestClient CreateRestClient() => _restClientFactory(_restClientParams);
    }
}