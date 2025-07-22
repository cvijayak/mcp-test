namespace CMS.Mcp.Shared.Common.Contracts
{
    using System;
    using System.IO;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public interface IJsonObjectSerializer
	{
		T Deserialize<T>(JsonReader reader);
		T Deserialize<T>(string jsonString);
        bool TryDeserialize<T>(string jsonString, out T obj);
        object Deserialize(string jsonString, Type type);
		string Serialize<T>(T objectToConvert);
		T Deserialize<T>(Stream stream);
        JToken FromObject(object obj);
    }
}