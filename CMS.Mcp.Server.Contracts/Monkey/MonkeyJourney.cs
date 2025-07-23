namespace CMS.Mcp.Server.Contracts.Monkey
{
    using System;
    using System.Collections.Generic;

    public class MonkeyJourney {
        public string MonkeyName { get; set; } = string.Empty;
        public GeoLocation StartLocation { get; set; } = new();
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<PathPoint> PathPoints { get; set; } = new();
        public List<MonkeyActivity> Activities { get; set; } = new();
        public MonkeyHealthStats HealthStats { get; set; } = new();
        public TimeSpan TotalDuration => EndTime - StartTime;
        public double TotalDistanceKm => CalculateTotalDistance();

        private double CalculateTotalDistance() {
            double total = 0;
            for (int i = 1; i < PathPoints.Count; i++) {
                total += PathPoints[i - 1].Location.DistanceTo(PathPoints[i].Location);
            }
            return total;
        }
    }
}