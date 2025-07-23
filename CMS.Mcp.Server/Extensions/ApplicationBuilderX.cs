namespace CMS.Mcp.Server.Extensions
{
    using Microsoft.AspNetCore.Builder;

    public static class ApplicationBuilderX
    {
        public static IApplicationBuilder UseMcpServer(this IApplicationBuilder builder)
        {
            return builder
                .UseAuthentication()
                .UseAuthorization();
        }
    }
}
