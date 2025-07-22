namespace CMS.Mcp.Shared.Common
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;

    public static class JsonSerializerConfiguration
	{
		private static readonly List<JsonConverter> JSON_CONVERTERS = [new StringEnumConverter()];

		public static CamelCasePropertyNamesContractResolver ContractResolver => new()
		{
			NamingStrategy = new CamelCaseNamingStrategy(false, false)
		};

		public static DateTimeZoneHandling DateTimeZoneHandling => DateTimeZoneHandling.Utc;

		public static Formatting Formatting => Formatting.Indented;

		public static NullValueHandling NullValueHandling => NullValueHandling.Ignore;

		public static IReadOnlyCollection<JsonConverter> Converters => JSON_CONVERTERS.ToArray();

		public static void AddConverters(params JsonConverter[] jsonConverters)
		{
			JSON_CONVERTERS.AddRange(jsonConverters);
		}

		public static JsonSerializerSettings CreateJsonSerializerSettings() => new()
		{
			ContractResolver = ContractResolver,
			DateTimeZoneHandling = DateTimeZoneHandling,
			Formatting = Formatting,
			NullValueHandling = NullValueHandling,
			Converters = new List<JsonConverter>(Converters)
		};

        public static JsonSerializer CreateSerializer()
        {
            var serializer = new JsonSerializer
            {
                ContractResolver = ContractResolver,
                DateTimeZoneHandling = DateTimeZoneHandling,
                Formatting = Formatting,
                NullValueHandling = NullValueHandling
            };

            foreach (var converter in JSON_CONVERTERS)
            {
                serializer.Converters.Add(converter);
            }

            return serializer;
        }
    }
}