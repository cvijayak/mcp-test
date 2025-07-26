namespace CMS.Mcp.Client.Security.Contracts
{
    public interface IClaimStore
    {
        string GetValue(string claimType);
        string[] GetValues(string claimType);
        void SetValue(string type, string value);
        void SetValues((string type, string value)[] claims);
    }
}