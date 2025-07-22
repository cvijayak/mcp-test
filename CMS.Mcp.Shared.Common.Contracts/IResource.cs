namespace CMS.Mcp.Shared.Common.Contracts
{
    using System.IO;

    public interface IResource
    {
        Stream GetStream();
    }
}