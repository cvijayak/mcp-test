namespace CMS.Mcp.Shared.Common.Resources
{
    using System.IO;
    using Contracts;

    internal record FileResource(string FilePath) : IResource
    {
        public Stream GetStream() => new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }
}