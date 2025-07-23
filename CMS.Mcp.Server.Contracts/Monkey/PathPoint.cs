namespace CMS.Mcp.Server.Contracts.Monkey
{
    using System;

    public class PathPoint {
        public GeoLocation Location { get; set; } = new();
        public DateTime Timestamp { get; set; }
    }
}