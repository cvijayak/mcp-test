namespace CMS.Mcp.Shared.Common.Contracts.Exceptions
{
    using System;

    public class ResourceException(ResourceErrorType errorType, string name, string message, Exception exception = null)
        : Exception(message, exception)
    {
        public string Name { get; } = name;
        public ResourceErrorType ErrorType { get; } = errorType;
    }
}