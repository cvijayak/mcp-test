namespace CMS.Mcp.Client.Security
{
    using System.Security.Claims;

    public static class ClaimNames
    {
        public const string NAME_IDENTIFIER = ClaimTypes.NameIdentifier;
        public const string USER_FIRST_NAME = ClaimTypes.GivenName;
        public const string USER_LAST_NAME = ClaimTypes.Surname;
        public const string USER_EMAIL = ClaimTypes.Email;

        public const string ACCESS_TOKEN = "access_token";
        public const string REFRESH_TOKEN = "refresh_token";
        public const string EXPIRES_AT = "expires_at";
    }
}