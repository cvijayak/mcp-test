namespace CMS.Mcp.Shared.Configuration.Sources
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Configuration.Json;
    using ConfigurationProvider = Providers.ConfigurationProvider;

    internal class ConfigurationSource : JsonConfigurationSource
	{
		public override IConfigurationProvider Build(IConfigurationBuilder builder)
		{
			EnsureDefaults(builder);
			return new ConfigurationProvider(this);
		}
	}
}