namespace CMS.Mcp.Client.Security.Extensions
{
    using System;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Contracts.Options;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.OAuth;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Shared.Api.Clients.Contracts.Responses;
    using Shared.Common;

    public static class OAuthConnectOptionsX
    {
        public static void ConfigureOAuthAuthenticationScheme(this OAuthOptions options, IdentityServerOptions identityServerOptions)
        {
            ArgumentNullException.ThrowIfNull(options, nameof(options));
            ArgumentNullException.ThrowIfNull(identityServerOptions, nameof(identityServerOptions));

            OidcConfigResponse GetConfiguration()
            {
                using var client = new HttpClient();
                client.BaseAddress = identityServerOptions.Authority;

                using var request = new HttpRequestMessage(HttpMethod.Get, identityServerOptions.OidcEndpoint.GetWellKnownConfigUri());
                request.Headers.Add("Accept", "application/json");

                using var response = client.Send(request);
                response.EnsureSuccessStatusCode();

                using var stream = response.Content.ReadAsStream();
                return JsonObjectSerializerFactory.Create().Deserialize<OidcConfigResponse>(stream);
            }

            var oidcConfig = GetConfiguration();

            options.ClientId = identityServerOptions.AuthorizationCode.ClientId;
            options.ClientSecret = identityServerOptions.AuthorizationCode.ClientSecret;
            options.CallbackPath = new PathString("/signin-oidc");

            options.AuthorizationEndpoint = oidcConfig.AuthorizationEndpoint.ToString();
            options.TokenEndpoint = oidcConfig.TokenEndpoint.ToString();
            options.UserInformationEndpoint = oidcConfig.UserinfoEndpoint.ToString();

            options.SaveTokens = true;

            options.ClaimActions.MapJsonKey(ClaimTypes.Name, UserClaims.NAME);
            options.ClaimActions.MapJsonKey(ClaimTypes.Email, UserClaims.EMAIL);
            options.ClaimActions.MapJsonKey(ClaimTypes.GivenName, UserClaims.GIVEN_NAME);
            options.ClaimActions.MapJsonKey(ClaimTypes.Surname, UserClaims.FAMILY_NAME);
            options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, UserClaims.SUBJECT);

            options.Scope.Clear();
            foreach (var scope in identityServerOptions.AuthorizationCode.Scopes.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                options.Scope.Add(scope);
            }

            options.Events = new OAuthEvents
            {
                OnCreatingTicket = async context =>
                {
                    var transformer = context.HttpContext.RequestServices.GetRequiredService<ClaimsTransformer>();
                    await transformer.TransformAsync(context);
                },
                OnRemoteFailure = context =>
                {
                    context.Response.Redirect("/mcp/home/error");
                    context.HandleResponse();

                    return Task.CompletedTask;
                }
            };
        }
    }
}
