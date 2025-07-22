namespace CMS.Mcp.Shared.Configuration.Providers
{
    using System;
    using System.Linq;
    using Microsoft.Extensions.Configuration.Json;

    internal class ConfigurationProvider(JsonConfigurationSource source) : JsonConfigurationProvider(source)
    {
        protected virtual string Convert(string configData) => configData;

		public override void Load()
		{
			base.Load();

			Data = Data.ToDictionary(x => x.Key, x => {
                if (x.Value == null)
                {
                    return x.Value;
                }

				var environmentVariable = Environment.GetEnvironmentVariable(x.Value.Trim('%'));
				if (environmentVariable == null)
				{
					return x.Value;
				}

				var environmentVariableValue = Environment.ExpandEnvironmentVariables(x.Value);
				return Convert(environmentVariableValue);
			}, StringComparer.OrdinalIgnoreCase);
		}
	}
}