namespace CMS.Mcp.Shared.Common.Contracts
{
    using System;
    using System.Reflection;

    public interface IResourceRegistry
	{
        void RegisterAssembly(string assemblyName, params Type[] types);
        void RegisterAssembly(Assembly assembly, params Type[] types);
        void RegisterType<T>(T resource);
        void RegisterFile(string filePath, Type type);
        IResource Get(string key);
    }
}