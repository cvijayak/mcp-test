namespace CMS.Mcp.Client.Contracts.Models 
{
    using System;

    public class McpToolViewModel
    {
        public string Title { get; set; }
        public string Name { get; set; } 
        public string Description { get; set; } 
        public McpToolParameter[] Parameters { get; set; } = Array.Empty<McpToolParameter>();
        //public bool IsEnabled { get; set; } = true;
    }
}