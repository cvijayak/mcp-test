namespace CMS.Mcp.Client.Security.Contracts.Providers
{
    using System;

    public interface IClaimStoreProvider
    {
        Guid? UserId { get; }
        string FirstName { get; }
        string LastName { get; }
        string FullName { get; }
        string Email { get; }
    }
}