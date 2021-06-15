using System.Collections.Generic;

namespace TheTravelingSalesman.Impl
{
    public class DoubleArrayProblem<L> : AbstractProblem<L>, IProblem<L>
    {
        private double[] distances;

        public DoubleArrayProblem(IList<L> locations, double[][] distances)
            : base(locations)
        {
            this.distances = ConvertDistances(distances, GetLocationsCount());
        }

        protected internal virtual double[] ConvertDistances(double[][] distances, int locationCount)
        {
            double[] result = new double[locationCount * locationCount];
            for (int i = 0; i < locationCount; i++)
            {
                for (int j = 0; j < locationCount; j++)
                {
                    result[i * locationCount + j] = distances[i][j];
                }
            }
            return result;
        }

        public virtual double GetDistance(int from, int to)
        {
            return distances[from * GetLocationsCount() + to];
        }

        public override void CalculateLengths(IList<IPath<L>> paths)
        {
            foreach (IPath<L> path in paths)
            {
                double result = 0;
                int[] locations = path.GetLocations();
                if (fixedFirstLocation >= 0 && locations.Length > 0 && locations[0] != fixedFirstLocation)
                {
                    ((DoublePath<L>)path).SetLength(double.PositiveInfinity);
                    continue;
                }
                for (int i = 1; i < locations.Length; i++)
                {
                    int from = locations[i - 1];
                    int to = locations[i];
                    result += distances[from * GetLocationsCount() + to];
                }
                ((DoublePath<L>)path).SetLength(result);
            }
        }

        public override IPath<L> CreatePath(int[] locations)
        {
            return new DoublePath<L>(this, locations);
        }

        public override string ToString()
        {
            return "ProblemArray [LocationsCount=" + GetLocationsCount() + "]";
        }
    }
}