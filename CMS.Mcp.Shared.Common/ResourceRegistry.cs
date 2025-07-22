namespace CMS.Mcp.Shared.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;
    using Contracts.Exceptions;
    using Extensions;
    using Resources;

    public class ResourceRegistry : IResourceRegistry 
	{
		private readonly Dictionary<string, IResource> _resourceInfoMap = [];

		private ResourceRegistry() { }

        public void RegisterAssembly(Assembly assembly, params Type[] types)
        {
            var resources = types.ToDictionary(x => x.Name, s => (IResource)new EmbeddedResource(assembly, $"{s.Name.ToCamelCase()}.json"));
            Register(resources);
        }

        public void RegisterAssembly(string assemblyName, params Type[] types)
        {
            var resourceAssembly = Assembly.Load(new AssemblyName(assemblyName));
            RegisterAssembly(resourceAssembly, types);
        }

        public void RegisterType<T>(T resource) => Register(typeof(T).Name, new JsonResource(resource.ToJson()));

        public void RegisterFile(string filePath, Type type)
        {
            Register(type.Name, new FileResource(filePath));
        }

        public IResource Get(string key) 
        {
            if (!_resourceInfoMap.TryGetValue(key, out var embeddedConfiguration)) 
            {
                throw new ResourceException(ResourceErrorType.UnknownResource, key, "Provided configuration type is not recognized or supported");
            }

            return embeddedConfiguration;
        }

        private void Register(string key, IResource resource) 
        {
            if (_resourceInfoMap.ContainsKey(key))
            {
                _resourceInfoMap.Remove(key);
            }

            _resourceInfoMap.Add(key, resource);
        }

        private void Register(Dictionary<string, IResource> resources)
        {
            foreach (var (key, registerInfo) in resources)
            {
                Register(key, registerInfo);
            }
        }

        public static IResourceRegistry Instance { get; } = new ResourceRegistry();
    }
}