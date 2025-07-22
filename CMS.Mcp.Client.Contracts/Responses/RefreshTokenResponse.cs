namespace CMS.Mcp.Client.Contracts.Responses
{
    using System;

    public class RefreshTokenResponse
    {
        public string AccessToken { get; init; }
        public DateTimeOffset? ExpiresAtUtc { get; init; }
    }
}