namespace CMS.Mcp.Client.Security
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;

    public class JwtToken
    {
        private readonly JwtSecurityToken _jwtToken;

        public JwtToken(string accessToken)
        {
            AccessToken = accessToken;

            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            _jwtToken = jwtSecurityTokenHandler.CanReadToken(accessToken) ? jwtSecurityTokenHandler.ReadJwtToken(accessToken) : null;
        }

        public DateTimeOffset? IssuedUtc
        {
            get
            {
                var issuedAtClaim = _jwtToken?.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Iat);
                if (issuedAtClaim != null && long.TryParse(issuedAtClaim.Value, out var issuedAtUnix))
                {
                    return DateTimeOffset.FromUnixTimeSeconds(issuedAtUnix).UtcDateTime;
                }

                return null;
            }
        }

        public DateTimeOffset? ExpiresAtUtc
        {
            get
            {
                var expirationClaim = _jwtToken?.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp);
                if (expirationClaim != null && long.TryParse(expirationClaim.Value, out var expUnix))
                {
                    return DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
                }

                return null;
            }
        }

        public string AccessToken { get; }

        public static implicit operator JwtToken(string accessToken) => new(accessToken);
    }
}