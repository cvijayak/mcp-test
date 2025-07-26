namespace CMS.Mcp.Server.Contracts.Monkey
{
    using System;

    public class MonkeyActivity
    {
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public GeoLocation Location { get; set; } = new();
        public DateTime Timestamp { get; set; }
        public TimeSpan Duration { get; set; }
        public int EnergyChange { get; set; }
    }
}