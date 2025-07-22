namespace CMS.Mcp.Shared.Common
{
    using System.IO;
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

		public static T FromJson<T>(this Stream stream, JsonSerializerSettings settings = null) => Create(settings).Deserialize<T>(stream);

        public static T FromJson<T>(this string json, JsonSerializerSettings settings = null) => Create(settings).Deserialize<T>(json);
    }
}