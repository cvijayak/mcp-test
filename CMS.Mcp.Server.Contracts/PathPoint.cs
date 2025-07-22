namespace CMS.Mcp.Server.Contracts
{
    using System;

    public class PathPoint {
        public GeoLocation Location { get; set; } = new();
        public DateTime Timestamp { get; set; }
    }
}