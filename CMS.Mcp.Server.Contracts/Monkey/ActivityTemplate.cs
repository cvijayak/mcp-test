namespace CMS.Mcp.Server.Contracts.Monkey
{
    public class ActivityTemplate
    {
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int MinDuration { get; set; }
        public int MaxDuration { get; set; }
        public int EnergyChange { get; set; }
    }
}