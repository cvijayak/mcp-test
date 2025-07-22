namespace CMS.Mcp.Client.Middlewares {
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    public class RedirectionMiddleware(RequestDelegate next)
    {
        public async Task Invoke(HttpContext context, ILogger<RedirectionMiddleware> logger)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant();

            if (path is "/" or "/mcp" or "/mcp/home" or "/mcp/home/")
            {
                if (context.User.Identity?.IsAuthenticated == true)
                {
                    logger.LogDebug($"Redirecting authenticated user from {path} to Chat");
                    context.Response.Redirect("/mcp/chat");
                    return;
                }

                logger.LogDebug($"Redirecting unauthenticated user from {path} to Login");
                context.Response.Redirect("/mcp/account/login");
                return;
            }

            await next(context);
        }
    }
}