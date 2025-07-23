namespace CMS.Mcp.Client.Security.Contracts
{
    public interface IClaimStore
    {
        string GetValue(string claimType);
    }
}