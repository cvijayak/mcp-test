namespace CMS.Mcp.Shared.Common.Contracts
{
    public interface IResourceReader
	{
		T ReadAsJson<T>() where T : IJsonResource;
        string ReadAsString(string configurationName);
	}
}