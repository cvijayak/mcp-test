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
    }
}