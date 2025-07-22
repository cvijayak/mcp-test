namespace CMS.Mcp.Shared.Api.Clients.Contracts.Responses
{
    using System;
    using Contracts;
    using Newtonsoft.Json;

    public class OidcConfigResponse : IApiResponse
	{
		[JsonProperty("authorization_endpoint")]
		public Uri AuthorizationEndpoint { get; set; }

		[JsonProperty("token_endpoint")]
		public Uri TokenEndpoint { get; set; }

		[JsonProperty("userinfo_endpoint")]
		public Uri UserinfoEndpoint { get; set; }

        [JsonProperty("end_session_endpoint")]
        public Uri EndSessionEndpoint { get; set; }
	}
}