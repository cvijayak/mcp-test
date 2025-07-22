namespace CMS.Mcp.Shared.Common.Extensions
{
    using System.Linq;
    using System.Security.Claims;

    public static class ClaimsPrincipalX
    {
        public static string GetClaimValue(this ClaimsPrincipal principal, string type)
        {
            return principal?.Claims.FirstOrDefault(c => c.Type == type)?.Value;
        }
    }
}