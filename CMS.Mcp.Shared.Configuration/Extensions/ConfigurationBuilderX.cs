namespace CMS.Mcp.Shared.Configuration.Extensions
{
    using System.IO;
    using SourceBuilders;
    using Common;
    using Microsoft.Extensions.Configuration;

    public static class ConfigurationBuilderX
	{
		private const string SECRET_APP_SETTINGS_FILE_PATH = "config/appsettings.json";
		private const string DEFAULT_APP_SETTINGS_FILE_PATH = "appsettings.{0}.json";
		private const string APP_SETTINGS_FILE_PATH = "appsettings.json";

		public static IConfigurationBuilder AddConfigurationSource(this IConfigurationBuilder configurationBuilder)
		{
            if (File.Exists(SECRET_APP_SETTINGS_FILE_PATH))
			{
                var resourceReader = ResourceReader.Create();
				var builder = new SecretConfigurationSourceBuilder(SECRET_APP_SETTINGS_FILE_PATH, resourceReader);
				return builder.Build(configurationBuilder);
			}

			var sourceBuilder = new ConfigurationSourceBuilder(DEFAULT_APP_SETTINGS_FILE_PATH, APP_SETTINGS_FILE_PATH);
			return sourceBuilder.Build(configurationBuilder);
		}
	}
}