namespace CMS.Mcp.Shared.Api.Clients.Contracts.Requests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using Contracts;

    public class PostFormApiRequest<T>() : ApiRequestBase(HttpMethod.Post) where T : IApiResponse
	{
		public IReadOnlyDictionary<string, string> Content { get; init; }
		public Func<Stream, T> Deserialize { get; init; }

        public override HttpRequestMessage CreateHttpRequestMessage()
        {
            var requestMessage = base.CreateHttpRequestMessage();
            requestMessage.Content = new FormUrlEncodedContent(Content.ToList());

            return requestMessage;
        }
    }
}