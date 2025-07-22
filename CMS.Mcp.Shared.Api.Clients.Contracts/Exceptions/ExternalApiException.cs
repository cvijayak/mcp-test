namespace CMS.Mcp.Shared.Api.Clients.Contracts.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using Responses;

    public class ExternalApiException : Exception
    {
        public ExternalApiException(ExternalApiErrorResponse errorResponse)
        {
            ErrorResponse = errorResponse;
        }

        public ExternalApiException(string serverType, HttpStatusCode status, Dictionary<string, string> responseHeaders, string detail)
        {
            ErrorResponse = new ExternalApiErrorResponse(status)
            {
                ExternalServerType = serverType,
                Detail = detail,
                ResponseHeaders = responseHeaders
            };
        }

        public override string ToString() => ErrorResponse?.ToString() ?? string.Empty;

        public ExternalApiErrorResponse ErrorResponse { get; }
    }
}