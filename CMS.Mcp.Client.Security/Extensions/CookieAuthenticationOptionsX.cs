namespace CMS.Mcp.Client.Security.Extensions
{
    using System;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.Extensions.DependencyInjection;

    public static class CookieAuthenticationOptionsX
    {
        public static void ConfigureCookieAuthenticationScheme(this CookieAuthenticationOptions options)
        {
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            options.LoginPath = "/mcp/account/login";
            options.LogoutPath = "/mcp/account/logout";
            options.AccessDeniedPath = "/mcp/home/error";
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(50);

            options.Events = new CookieAuthenticationEvents
            {
                OnValidatePrincipal = async context =>
                {
                    var sessionManager = context.HttpContext.RequestServices.GetRequiredService<TokenSessionManager>();
                    await sessionManager.RefreshSessionAsync(context);
                }
            };
        }
    }
}