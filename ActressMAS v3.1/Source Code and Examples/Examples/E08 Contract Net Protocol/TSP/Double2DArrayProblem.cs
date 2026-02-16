using System.Collections.Generic;

namespace TheTravelingSalesman.Impl
{
    public class Double2DArrayProblem<T> : AbstractProblem<T>, IProblem<T>
    {
        private double[][] distances;

        public Double2DArrayProblem(IList<T> locations, double[][] distances)
            : base(locations)
        {
            this.distances = distances;
        }

        public virtual double GetDistance(int from, int to)
        {
            return distances[from][to];
        }

        public override void CalculateLengths(IList<IPath<T>> paths)
        {
            foreach (IPath<T> path in paths)
            {
                double result = 0;
                int[] locations = path.GetLocations();
                if (fixedFirstLocation >= 0 && locations.Length > 0 && locations[0] != fixedFirstLocation)
                {
                    ((DoublePath<T>)path).SetLength(double.PositiveInfinity);
                    continue;
                }
                for (int i = 1; i < locations.Length; i++)
                {
                    int from = locations[i - 1];
                    int to = locations[i];
                    result += distances[from][to];
                }
                ((DoublePath<T>)path).SetLength(result);
            }
        }

        public override IPath<T> CreatePath(int[] locations)
        {
            return new DoublePath<T>(this, locations);
        }
    }
}