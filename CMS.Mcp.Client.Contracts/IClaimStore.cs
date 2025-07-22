namespace CMS.Mcp.Client.Contracts
{
    public interface IClaimStore
    {
        string GetValue(string claimType);
    }
}