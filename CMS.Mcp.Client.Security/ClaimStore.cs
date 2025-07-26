namespace CMS.Mcp.Client.Security
{
    using System.Security.Claims;
    using Contracts;
    using Microsoft.AspNetCore.Http;
    using Shared.Common.Extensions;

    public class ClaimStore(IHttpContextAccessor httpContextAccessor) : IClaimStore 
    {
        private readonly ClaimsPrincipal _claimsPrincipal = httpContextAccessor.HttpContext?.User;

        public string GetValue(string claimType) => _claimsPrincipal?.GetClaimValue(claimType);
        public string[] GetValues(string claimType) => _claimsPrincipal?.GetClaimValues(claimType);
        public void SetValue(string type, string value) => _claimsPrincipal?.AddClaim(type, value);
        public void SetValues((string type, string value)[] claims) => _claimsPrincipal?.AddClaims(claims);
    }
}