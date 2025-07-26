namespace CMS.Mcp.Client.Security.Providers
{
    using System;
    using Contracts;
    using Contracts.Providers;

    public class ClaimStoreProvider(IClaimStore claimStore) : IClaimStoreProvider
    {
        private Guid? GetGuidValue(string claimName)
        {
            var claimValue = claimStore?.GetValue(claimName);
            return string.IsNullOrEmpty(claimValue) ? null : Guid.Parse(claimValue);
        }

        public Guid? UserId => GetGuidValue(ClaimNames.NAME_IDENTIFIER);
        public string Email => claimStore?.GetValue(ClaimNames.USER_EMAIL);
        public string FirstName => claimStore?.GetValue(ClaimNames.USER_FIRST_NAME);
        public string LastName => claimStore?.GetValue(ClaimNames.USER_LAST_NAME);
        public string FullName => string.Join(" ", FirstName ?? string.Empty, LastName ?? string.Empty).Trim();
    }
}