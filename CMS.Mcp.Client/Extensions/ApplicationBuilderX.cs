namespace CMS.Mcp.Client.Extensions
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.AspNetCore.StaticFiles;
    using Middlewares;

    public static class ApplicationBuilderX
    {
        public static IApplicationBuilder UseMcpAgent(this IApplicationBuilder builder)
        {
            var contentTypeProvider = new FileExtensionContentTypeProvider();

            builder
                .UseStaticFiles(new StaticFileOptions {
                    ContentTypeProvider = contentTypeProvider,
                    OnPrepareResponse = ctx => ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=3600")
                })
                .UseStaticFiles(new StaticFileOptions {
                    RequestPath = "/mcp",
                    ContentTypeProvider = contentTypeProvider,
                    OnPrepareResponse = ctx => ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=3600")
                })
                .UseExceptionHandler("/mcp/home/error")
                .UseStatusCodePagesWithReExecute("/mcp/home/error");

            return builder
                .UseSecurity()
                .UseRouting()
                .UseAuthentication()
                .UseAuthorization()
                .UseMiddleware<HttpForwardingMiddleware>()
                .UseMiddleware<RedirectionMiddleware>()
                .UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto })
                .UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = SameSiteMode.None });
            ;
        }

        private static IApplicationBuilder UseSecurity(this IApplicationBuilder builder) 
        {
            return builder
                .UseCors()
                .UseHsts(options => options.MaxAge(365).IncludeSubdomains())
                .UseXContentTypeOptions()
                .UseReferrerPolicy(options => options.NoReferrer())
                .UseXXssProtection(options => options.EnabledWithBlockMode())
                .UseXfo(options => options.Deny());
        }
    }
}
