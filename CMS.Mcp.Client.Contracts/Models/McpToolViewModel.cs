namespace CMS.Mcp.Client.Contracts.Models 
{
    using System;

    public class McpToolViewModel
    {
        public class McpToolParameter
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Type { get; set; }
        }

        public string Title { get; set; }
        public string Name { get; set; } 
        public string Description { get; set; } 
        public McpToolParameter[] Parameters { get; set; } = Array.Empty<McpToolParameter>();
    }
}