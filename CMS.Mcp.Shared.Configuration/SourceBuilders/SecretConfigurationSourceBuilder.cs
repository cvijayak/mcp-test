namespace CMS.Mcp.Shared.Configuration.SourceBuilders
{
    using System.Linq;
    using Sources;
    using Common.Contracts;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Configuration.Json;

    internal class SecretConfigurationSourceBuilder(string configFilePath, IResourceReader resourceReader)
    {
        public IConfigurationBuilder Build(IConfigurationBuilder configurationBuilder)
		{
			var sources = configurationBuilder.Sources.OfType<JsonConfigurationSource>().ToArray();
			foreach (var source in sources)
			{
				configurationBuilder.Sources.Remove(source);
			}

			var configurationSource = new SecretConfigurationSource(resourceReader)
			{
				FileProvider = null,
				Path = configFilePath,
				Optional = false,
				ReloadOnChange = true
			};

			configurationBuilder.Sources.Add(configurationSource);

			return configurationBuilder;
		}
	}
}