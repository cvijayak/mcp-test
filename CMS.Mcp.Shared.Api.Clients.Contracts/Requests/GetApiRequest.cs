namespace CMS.Mcp.Shared.Api.Clients.Contracts.Requests
{
    using System;
    using System.IO;
    using System.Net.Http;
    using Contracts;

    public class GetApiRequest<T>() : ApiRequestBase(HttpMethod.Get) where T : IApiResponse
	{
		public Func<Stream, T> Deserialize { get; init; }
    }
}