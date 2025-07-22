namespace CMS.Mcp.Client.Contracts.Models 
{
    using System;

    public class McpToolViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string[] Parameters { get; set; } = Array.Empty<string>();
        //public bool IsEnabled { get; set; } = true;
    }
}