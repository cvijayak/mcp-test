namespace CMS.Mcp.Server.Contracts.Monkey
{
    public class MonkeyHealthStats
    {
        public int Energy { get; set; } // 0-100
        public int Happiness { get; set; } // 0-100
        public int Hunger { get; set; } // 0-100 (higher = more hungry)
        public int Social { get; set; } // 0-100
        public int Stress { get; set; } // 0-100 (higher = more stressed)
        public int Health { get; set; } // 0-100
    }
}