namespace CMS.Mcp.Shared.Api.Clients.Contracts.Responses
{
    using Contracts;
    using Newtonsoft.Json;

    public class AuthTokenResponse : IApiResponse
	{
		[JsonProperty("access_token")]
		public string AccessToken { get; init; }

        [JsonProperty("token_type")]
        public string TokenType { get; init; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; init; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; init; }
    }
}