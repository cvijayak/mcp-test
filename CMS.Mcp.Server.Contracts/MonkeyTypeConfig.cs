namespace CMS.Mcp.Server.Contracts
{
    using System.Collections.Generic;

    public class MonkeyTypeConfig {
        public double MaxMovementRadius { get; set; }
        public string PreferredTerrain { get; set; } = string.Empty;
        public int BaseEnergy { get; set; }
        public int BaseSocial { get; set; }
        public List<ActivityTemplate> PreferredActivities { get; set; } = new();
    }
}