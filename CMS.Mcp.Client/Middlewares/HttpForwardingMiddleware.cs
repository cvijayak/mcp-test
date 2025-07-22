namespace CMS.Mcp.Client.Middlewares
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    internal class HttpForwardingMiddleware(RequestDelegate next)
    {
        public async Task Invoke(HttpContext context, ILogger<HttpForwardingMiddleware> logger)
        {
            var protoForwardHeaderValue = context.Request.Headers["X-Forwarded-Proto"].FirstOrDefault();
            var scheme = context.Request.Scheme;

            if (string.Equals("https", protoForwardHeaderValue, StringComparison.Ordinal) && string.Equals("http", context.Request.Scheme))
            {
                context.Request.Scheme = "https";

                logger.LogDebug($"Changed the schema, OldScheme: {scheme}, NewScheme: {context.Request.Scheme}");
            }
            else if (string.Equals("http", protoForwardHeaderValue, StringComparison.Ordinal) && string.Equals("https", context.Request.Scheme))
            {
                context.Request.Scheme = "http";

                logger.LogDebug($"Changed the schema, OldScheme: {scheme}, NewScheme: {context.Request.Scheme}");
            }

            await next(context);
        }
    }
}