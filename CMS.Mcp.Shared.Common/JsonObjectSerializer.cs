namespace CMS.Mcp.Shared.Common
{
    using System;
    using System.IO;
    using Contracts;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class JsonObjectSerializer(JsonSerializerSettings settings) : IJsonObjectSerializer
    {
        public T Deserialize<T>(JsonReader reader)
		{
			var serializer = JsonSerializer.Create(settings);
			return serializer.Deserialize<T>(reader);
		}

		public T Deserialize<T>(string jsonString) => JsonConvert.DeserializeObject<T>(jsonString, settings);

        public bool TryDeserialize<T>(string jsonString, out T obj)
        {
            try
            {
                obj = JsonConvert.DeserializeObject<T>(jsonString, settings);
                return true;
            }
            catch
            {
                obj = default;
                return false;
            }
        }

        public object Deserialize(string jsonString, Type type) => JsonConvert.DeserializeObject(jsonString, type, settings);

		public T Deserialize<T>(Stream stream)
		{
			using var streamReader = new StreamReader(stream);
			using var reader = new JsonTextReader(streamReader);
			return Deserialize<T>(reader);
		}

		public string Serialize<T>(T objectToConvert) => JsonConvert.SerializeObject(objectToConvert, settings);

        public JToken FromObject(object obj)
        {
            var serializer = JsonSerializer.Create(settings);
            return JToken.FromObject(obj, serializer);
        }
    }
}