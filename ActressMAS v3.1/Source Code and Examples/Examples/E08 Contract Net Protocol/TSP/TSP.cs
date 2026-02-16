using System;
using System.Collections.Generic;

namespace TheTravelingSalesman
{
    public class Location
    {
        public string Name { get; private set; }

        public double Latitude { get; private set; }

        public double Longitude { get; private set; }

        public Location(string name, double lat, double lon)
        {
            Name = name;
            Latitude = lat;
            Longitude = lon;
        }
    }

    public class TSP
    {
        public static IPath<Location> Solve(IList<Location> locations)
        {
            IProblem<Location> problem = new ProblemBuilder<Location>(typeof(Location))
                .Locations(locations)
                .Distances((Location loc1, Location loc2) => DistFrom(loc1.Latitude, loc1.Longitude, loc2.Latitude, loc2.Longitude))
                .FixedFirstLocation(0)
                .BuildIntegerArray();

            ISolver<Location> solver = new NearestNeighborSolver<Location>(problem);
            IPath<Location> path = solver.Solve();
            return path;
        }

        private static double DistFrom(double lat1, double lng1, double lat2, double lng2)
        {
            double earthRadius = 6371000; //meters
            double dLat = ToRadians(lat2 - lat1);
            double dLng = ToRadians(lng2 - lng1);
            double a = System.Math.Sin(dLat / 2) * System.Math.Sin(dLat / 2) + System.Math.Cos(ToRadians(lat1)) * System.Math.Cos(ToRadians(lat2)) * System.Math.Sin(dLng / 2) * System.Math.Sin(dLng / 2);
            double c = 2 * System.Math.Atan2(System.Math.Sqrt(a), System.Math.Sqrt(1 - a));
            double dist = (float)(earthRadius * c);
            return dist;
        }

        private static double ToRadians(double angle)
        {
            return Math.PI / 180.0 * angle;
        }
    }
}