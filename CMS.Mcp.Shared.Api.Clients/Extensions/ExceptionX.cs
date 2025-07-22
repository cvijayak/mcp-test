namespace CMS.Mcp.Shared.Api.Clients.Extensions
{
    using System;
    using System.Net;
    using Common.Extensions;
    using Contracts.Exceptions;

    internal static class ExceptionX
    {
        public static ExternalApiException BuildServiceUnavailable(this Exception exception, string serverType) =>
            exception.Build(serverType, HttpStatusCode.ServiceUnavailable);

        public static ExternalApiException BuildGatewayTimeout(this Exception exception, string serverType) =>
            exception.Build(serverType, HttpStatusCode.GatewayTimeout);

        public static ExternalApiException Build(this Exception exception, string serverType, HttpStatusCode status) => 
            new(serverType, status, null, exception.ToDemystifiedString());
    }
}