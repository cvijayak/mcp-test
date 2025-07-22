namespace CMS.Mcp.Shared.Common.Extensions
{
    using System.Linq;
    using System.Reflection;

    internal static class AssemblyX
	{
		public static bool IsManifestResourceAvailable(this Assembly assembly, string resourceName) => assembly.GetManifestResourceNames().Any(name => name == resourceName);
	}
}