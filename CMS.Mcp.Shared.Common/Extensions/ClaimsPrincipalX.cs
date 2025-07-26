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

        public static string[] GetClaimValues(this ClaimsPrincipal principal, string type)
        {
            var claimValues = principal?.Claims.Where(s => s.Type == type).Select(c => c.Value).ToArray();
            return claimValues == null || !claimValues.Any() ? null : claimValues;
        }

        public static void AddClaim(this ClaimsPrincipal principal, string type, string value)
        {
            var claimsIdentity = principal?.Identity as ClaimsIdentity;
            claimsIdentity?.AddClaim(new Claim(type, value));
        }

        public static void AddClaims(this ClaimsPrincipal principal, (string type, string value)[] claims)
        {
            var claimsIdentity = principal?.Identity as ClaimsIdentity;
            claimsIdentity?.AddClaims(claims.Select(f => new Claim(f.type, f.value)).ToArray());
        }
    }
}