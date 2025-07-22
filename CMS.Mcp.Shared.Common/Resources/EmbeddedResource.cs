namespace CMS.Mcp.Shared.Common.Resources
{
    using System.IO;
    using System.Reflection;
    using Contracts;
    using Extensions;

    internal record EmbeddedResource(Assembly Assembly, string FileName) : IResource
    {
        private string FileFullName => $"{Assembly.GetName().Name}.{FileName}";

        public Stream GetStream() => !Assembly.IsManifestResourceAvailable(FileFullName) ? null : Assembly.GetManifestResourceStream(FileFullName);
    }
}