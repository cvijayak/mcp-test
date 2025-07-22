namespace CMS.Mcp.Shared.Configuration.SourceBuilders
{
    using System;
    using System.IO;
    using System.Linq;
    using Sources;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Configuration.Json;

    internal class ConfigurationSourceBuilder(string baseConfigFilePath, string configFilePath)
    {
        public IConfigurationBuilder Build(IConfigurationBuilder configurationBuilder)
		{
			var sources = configurationBuilder.Sources.OfType<JsonConfigurationSource>().ToArray();
			var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environment.GetEnvironmentVariable("ENVIRONMENT_NAME");
			var appSettingsPath = !string.IsNullOrEmpty(environmentName) ? string.Format(baseConfigFilePath, environmentName) : configFilePath;

			if (File.Exists(appSettingsPath) && sources.All(c => c.Path != appSettingsPath))
			{
				configurationBuilder.AddJsonFile(appSettingsPath, true, true);
				sources = configurationBuilder.Sources.OfType<JsonConfigurationSource>().ToArray();
			}

			foreach (var source in sources)
			{
				var configSource = configurationBuilder.Sources.IndexOf(source);

				configurationBuilder.Sources.RemoveAt(configSource);
				var newConfigSource = new ConfigurationSource
				{
					FileProvider = source.FileProvider,
					Path = source.Path,
					Optional = source.Optional,
					ReloadOnChange = source.ReloadOnChange
				};
				configurationBuilder.Sources.Insert(configSource, newConfigSource);
			}

			return configurationBuilder;
		}
	}
}