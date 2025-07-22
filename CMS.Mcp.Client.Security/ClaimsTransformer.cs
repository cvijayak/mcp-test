namespace CMS.Mcp.Client.Security
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication.OAuth;

    public class ClaimsTransformer
    {
        public async Task TransformAsync(OAuthCreatingTicketContext context)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));

            using var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

            using var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
            response.EnsureSuccessStatusCode();

            using var jsonDocument = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var user = jsonDocument.RootElement;

            context.RunClaimActions(user);
        }
    }
}