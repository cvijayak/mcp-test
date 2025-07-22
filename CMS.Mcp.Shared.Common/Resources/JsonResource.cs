namespace CMS.Mcp.Shared.Common.Resources
{
    using System.IO;
    using System.Text;
    using Contracts;

    internal class JsonResource(string json) : IResource
    {
        public Stream GetStream() => new MemoryStream(Encoding.UTF8.GetBytes(json));
    }
}