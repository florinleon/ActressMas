using System.Collections.Generic;

namespace TheTravelingSalesman.Impl
{
    public class IntegerArrayProblem<L> : AbstractProblem<L>, IProblem<L>
    {
        protected internal int[] distances;

        public IntegerArrayProblem(IList<L> locations, int[][] distances)
            : base(locations)
        {
            this.distances = ConvertDistances(distances, GetLocationsCount());
        }

        protected internal virtual int[] ConvertDistances(int[][] distances, int locationCount)
        {
            int[] result = new int[locationCount * locationCount];
            for (int i = 0; i < locationCount; i++)
            {
                for (int j = 0; j < locationCount; j++)
                {
                    result[i * locationCount + j] = distances[i][j];
                }
            }
            return result;
        }

        public virtual int GetDistance(int from, int to)
        {
            return distances[from * GetLocationsCount() + to];
        }

        public override void CalculateLengths(IList<IPath<L>> paths)
        {
            foreach (IPath<L> path in paths)
            {
                int result = 0;
                int[] locations = path.GetLocations();
                if (fixedFirstLocation >= 0 && locations.Length > 0 && locations[0] != fixedFirstLocation)
                {
                    ((IntegerPath<L>)path).SetLength(int.MaxValue);
                    continue;
                }
                for (int i = 1; i < locations.Length; i++)
                {
                    int from = locations[i - 1];
                    int to = locations[i];
                    result += distances[from * GetLocationsCount() + to];
                }
                ((IntegerPath<L>)path).SetLength(result);
            }
        }

        public override IPath<L> CreatePath(int[] locations)
        {
            return new IntegerPath<L>(this, locations);
        }

        public override string ToString()
        {
            return "ProblemArray [LocationsCount=" + GetLocationsCount() + "]";
        }
    }
}