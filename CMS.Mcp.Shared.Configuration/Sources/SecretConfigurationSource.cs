namespace CMS.Mcp.Shared.Configuration.Sources 
{
    using Common.Contracts;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Configuration.Json;
    using Providers;

    internal class SecretConfigurationSource(IResourceReader resourceReader) : JsonConfigurationSource
    {
        public override IConfigurationProvider Build(IConfigurationBuilder builder)
		{
			EnsureDefaults(builder);
			return new SecretConfigurationProvider(this, resourceReader);
		}
	}
}