namespace CMS.Mcp.Client.Security
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.Extensions.Logging;
    using Shared.Api.Clients.Contracts;
    using Shared.Api.Clients.Contracts.Responses;

    public class TokenSessionManager(IIdentityApiClient identityApiClient, ILogger<TokenSessionManager> logger)
    {
        private const int TOKEN_REFRESH_LEAD_TIME = -15;

        private bool UpdateAuthenticationProperties(AuthenticationProperties properties, RefreshTokenResponse response)
        {
            JwtToken jwtToken = response.AccessToken;
            var issuedUtc = jwtToken.IssuedUtc;
            var expiresAtUtc = jwtToken.ExpiresAtUtc;
            if (issuedUtc != null && expiresAtUtc != null)
            {
                properties.StoreTokens([
                    new AuthenticationToken { Name = ClaimNames.ACCESS_TOKEN, Value = response.AccessToken },
                    new AuthenticationToken { Name = ClaimNames.REFRESH_TOKEN, Value = response.RefreshToken },
                    new AuthenticationToken { Name = ClaimNames.EXPIRES_AT, Value = expiresAtUtc.Value.ToString("o") }
                ]);

                properties.IsPersistent = true;
                properties.IssuedUtc = issuedUtc;
                properties.ExpiresUtc = expiresAtUtc;

                return true;
            }

            return false;
        }

        private async Task<bool> SignInAsync(CookieValidatePrincipalContext context, RefreshTokenResponse response)
        {
            if (string.IsNullOrEmpty(response.AccessToken) || string.IsNullOrEmpty(response.RefreshToken) || response.ExpiresIn <= 0)
            {
                return false;
            }

            if (!UpdateAuthenticationProperties(context.Properties, response))
            {
                return false;
            }

            await context.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, context.Principal, context.Properties);
            return true;
        }

        public async Task RefreshSessionAsync(CookieValidatePrincipalContext context)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));

            var accessToken = context.Properties.GetTokenValue(ClaimNames.ACCESS_TOKEN);
            var refreshToken = context.Properties.GetTokenValue(ClaimNames.REFRESH_TOKEN);
            if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(refreshToken))
            {
                if (context is { Principal: not null })
                {
                    if (context.Properties.IsPersistent != true || context.Properties.IssuedUtc == null)
                    {
                        var tokenResponse = new RefreshTokenResponse
                        {
                            AccessToken = accessToken,
                            RefreshToken = refreshToken
                        };

                        if (await SignInAsync(context, tokenResponse))
                        {
                            return;
                        }
                    }

                    var expiresAt = context.Properties.GetTokenValue(ClaimNames.EXPIRES_AT);
                    if (!string.IsNullOrEmpty(expiresAt) && DateTimeOffset.TryParse(expiresAt, out DateTimeOffset expiresAtDateTime))
                    {
                        if (DateTimeOffset.UtcNow < expiresAtDateTime.AddMinutes(TOKEN_REFRESH_LEAD_TIME))
                        {
                            return;
                        }

                        try
                        {
                            var response = await identityApiClient.RefreshAccessTokenAsync(refreshToken, context.HttpContext.RequestAborted);
                            if (await SignInAsync(context, response))
                            {
                                return;
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Failed to refresh access token");
                        }
                    }
                }

                context.RejectPrincipal();

                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                context.Response.Redirect(context.Options.LoginPath);
            }
        }
    }
}