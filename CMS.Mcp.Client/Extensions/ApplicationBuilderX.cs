namespace CMS.Mcp.Client.Extensions {
    using Microsoft.AspNetCore.Builder;
    using Middlewares;

    public static class ApplicationBuilderX
    {
        public static IApplicationBuilder UseMcpClient(this IApplicationBuilder builder)
        {
            builder
                .UseExceptionHandler("/mcp/home/error")
                .UseStatusCodePagesWithReExecute("/mcp/home/error");

            return builder
                .UseSecurity()
                .UseRouting()
                .UseAuthentication()
                .UseAuthorization()
                .UseMiddleware<HttpForwardingMiddleware>()
                .UseMiddleware<RedirectionMiddleware>();
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
