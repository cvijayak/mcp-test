namespace CMS.Mcp.Client.Contracts
{
    using System.Collections.Generic;

    public class ExecuteToolRequest
    {
        public string ToolName { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
    }
}