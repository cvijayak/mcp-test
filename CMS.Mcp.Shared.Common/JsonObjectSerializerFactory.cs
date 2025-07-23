namespace CMS.Mcp.Shared.Common
{
    using Contracts;
    using Newtonsoft.Json;

    public static class JsonObjectSerializerFactory
	{
		public static IJsonObjectSerializer Create(JsonSerializerSettings settings = null)
		{
			var serializerSettings = settings ?? JsonSerializerConfiguration.CreateJsonSerializerSettings();
			return new JsonObjectSerializer(serializerSettings);
		}

		public static string ToJson<T>(this T obj, JsonSerializerSettings settings = null) => Create(settings).Serialize(obj);
    }
}