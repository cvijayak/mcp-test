namespace CMS.Mcp.Server.Contracts
{
    using System;

    public class GeoLocation {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public GeoLocation() { }

        public GeoLocation(double latitude, double longitude) {
            Latitude = latitude;
            Longitude = longitude;
        }

        public double DistanceTo(GeoLocation other) {
            // Haversine formula for distance calculation
            const double R = 6371; // Earth's radius in kilometers
            var dLat = (other.Latitude - Latitude) * Math.PI / 180;
            var dLon = (other.Longitude - Longitude) * Math.PI / 180;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(Latitude * Math.PI / 180) * Math.Cos(other.Latitude * Math.PI / 180) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
    }
}