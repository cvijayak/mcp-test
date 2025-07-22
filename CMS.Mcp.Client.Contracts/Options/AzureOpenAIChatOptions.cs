namespace CMS.Mcp.Client.Contracts.Options
{
    using System;

    public class AzureOpenAIChatOptions
    {
        public string DeploymentName { get; init; } 
        public Uri Endpoint { get; init; } 
        public string ApiKey { get; init; }
    }
}