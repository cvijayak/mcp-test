namespace CMS.Mcp.Shared.Common
{
    using System;
    using System.IO;
    using Contracts;
    using Contracts.Exceptions;

    public class ResourceReader(IResourceRegistry resourceRegistry, IJsonObjectSerializer jsonObjectSerializer) : IResourceReader
    {
        private Stream CreateResourceStream(string configurationName)
        {
            var resource = resourceRegistry.Get(configurationName);
            return resource.GetStream();
		}

		private T ReadAsJson<T>(string configurationName) where T : IJsonResource
		{
			using var stream = CreateResourceStream(configurationName);

			try
			{
				return jsonObjectSerializer.Deserialize<T>(stream);
			}
			catch (Exception exception)
			{
				throw new ResourceException
				(
					ResourceErrorType.UnableToSerializeResource, 
					configurationName, 
					$"Unable to serialize the resource to the type, Type : {typeof(T).Name}", 
					exception
				);
			}
		}

		public T ReadAsJson<T>() where T : IJsonResource => ReadAsJson<T>(typeof(T).Name);

        public string ReadAsString(string configurationName)
		{
			using var stream = CreateResourceStream(configurationName);
			using var streamReader = new StreamReader(stream);
			return streamReader.ReadToEnd();
		}

        public static IResourceReader Create()
        {
            var resourceRegistry = ResourceRegistry.Instance;
            var jsonSerializer = JsonObjectSerializerFactory.Create();

            return new ResourceReader(resourceRegistry, jsonSerializer);
        }
    }
}