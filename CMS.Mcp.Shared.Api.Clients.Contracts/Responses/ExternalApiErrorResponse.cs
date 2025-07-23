namespace CMS.Mcp.Shared.Api.Clients.Contracts.Responses
{
    using System.Collections.Generic;
    using System.Net;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;

    public class ExternalApiErrorResponse : ProblemDetails, IApiResponse
    {
		public ExternalApiErrorResponse(HttpStatusCode statusCode)
		{
			Status = (int)statusCode;
		}

		[JsonProperty("externalServerType")]
		public string ExternalServerType { get; init; }

		[JsonProperty("responseHeaders")]
		public Dictionary<string, string> ResponseHeaders { get; init; }
	}
}