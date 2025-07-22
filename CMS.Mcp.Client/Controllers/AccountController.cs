namespace CMS.Mcp.Client.Controllers 
{
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.Providers;
    using Contracts.Responses;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Shared.Api.Clients.Contracts;

    [Route("mcp/account")]
    public class AccountController(IHttpContextAccessor httpContextAccessor,
        ISessionProvider sessionProvider,
        IIdentityApiClient identityApiClient) : Controller 
    {
        [Route("logout")]
        [Authorize(AuthenticationSchemes = AuthSchemeNames.AUTHENTICATION_OIDC_SCHEME_NAME)]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext == null) 
            {
                return StatusCode((int) HttpStatusCode.Forbidden, new ProblemDetails 
                {
                    Status = (int) HttpStatusCode.Forbidden,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                    Title = "Permission Denied",
                    Detail = "Session has expired or is invalid",
                    Instance = HttpContext.Request.Path.Value
                });
            }

            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var oidcConfig = await identityApiClient.GetConfigurationAsync(cancellationToken);

            return Redirect(oidcConfig.EndSessionEndpoint.ToString());
        }

        [HttpGet]
        [Route("check-session")]
        [Authorize(AuthenticationSchemes = AuthSchemeNames.AUTHENTICATION_OIDC_SCHEME_NAME)]
        public IActionResult CheckSession()
        {
            return User.Identity?.IsAuthenticated == true ? Ok(new { message = "Session is active." }) : Unauthorized(new { message = "Session has expired." });
        }
        
        [HttpGet]
        [Route("login")]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return Redirect("/mcp/chat");
            }
            
            return Challenge(new AuthenticationProperties { RedirectUri = returnUrl ?? "/mcp/chat" });
        }

        [HttpPost]
        [Route("refresh-token")]
        [Authorize(AuthenticationSchemes = AuthSchemeNames.AUTHENTICATION_OIDC_SCHEME_NAME)]
        public async Task<IActionResult> RefreshToken(CancellationToken cancellationToken)
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext == null) 
            {
                return StatusCode((int) HttpStatusCode.Forbidden, new ProblemDetails 
                {
                    Status = (int) HttpStatusCode.Forbidden,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                    Title = "Permission Denied",
                    Detail = "Session has expired or is invalid",
                    Instance = HttpContext.Request.Path.Value
                });
            }

            var accessToken = await sessionProvider.GetAccessTokenAsync();
            var expiresAtUtc = await sessionProvider.GetExpiresAtUtcAsync();
            if (string.IsNullOrEmpty(accessToken) || expiresAtUtc == null) 
            {
                return StatusCode((int) HttpStatusCode.Forbidden, new ProblemDetails 
                {
                    Status = (int) HttpStatusCode.Forbidden,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                    Title = "Permission Denied",
                    Detail = "No refresh token found. Please re-authenticate to continue",
                    Instance = HttpContext.Request.Path.Value
                });
            }

            return Ok(new RefreshTokenResponse 
            {
                AccessToken = accessToken,
                ExpiresAtUtc = expiresAtUtc
            });
        }
    }
}
