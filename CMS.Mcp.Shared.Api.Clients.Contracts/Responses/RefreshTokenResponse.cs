namespace CMS.Mcp.Shared.Api.Clients.Contracts.Responses
{
    using Newtonsoft.Json;

    public class RefreshTokenResponse : AuthTokenResponse
    {
        [JsonProperty(PropertyName = "scope")]
        public string Scope { get; set; }

        [JsonProperty(PropertyName = "id_token")]
        public string IdToken { get; set; }
    }
}