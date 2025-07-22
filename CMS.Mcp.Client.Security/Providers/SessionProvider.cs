namespace CMS.Mcp.Client.Security.Providers 
{
    using System;
    using System.Threading.Tasks;
    using Contracts.Providers;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Http;

    public class SessionProvider(IHttpContextAccessor httpContextAccessor) : ISessionProvider
    {
        private async Task<string> GetTokenValueAsync(string tokenName)
        {
            if (httpContextAccessor?.HttpContext == null)
            {
                return null;
            }

            var authenticateResult = await httpContextAccessor.HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!authenticateResult.Succeeded)
            {
                return null;
            }

            return authenticateResult.Properties.GetTokenValue(tokenName);
        }

        public async Task<string> GetAccessTokenAsync() => await GetTokenValueAsync(ClaimNames.ACCESS_TOKEN);
        public async Task<DateTimeOffset?> GetExpiresAtUtcAsync()
        {
            var expiresAt = await GetTokenValueAsync(ClaimNames.EXPIRES_AT);
            return DateTimeOffset.TryParse(expiresAt, out DateTimeOffset expiresAtDateTime) ? expiresAtDateTime : null;
        }
    }
}
