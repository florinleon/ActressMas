using System.Collections.Generic;

namespace TheTravelingSalesman.Impl
{
    public class Integer2DArrayProblem<T> : AbstractProblem<T>, IProblem<T>
    {
        private int[][] distances;

        public Integer2DArrayProblem(IList<T> locations, int[][] distances)
            : base(locations)
        {
            this.distances = distances;
        }

        public virtual int GetDistance(int from, int to)
        {
            return distances[from][to];
        }

        public override void CalculateLengths(IList<IPath<T>> paths)
        {
            foreach (IPath<T> path in paths)
            {
                int result = 0;
                int[] locations = path.GetLocations();
                if (fixedFirstLocation >= 0 && locations.Length > 0 && locations[0] != fixedFirstLocation)
                {
                    ((IntegerPath<T>)path).SetLength(int.MaxValue);
                    continue;
                }
                for (int i = 1; i < locations.Length; i++)
                {
                    int from = locations[i - 1];
                    int to = locations[i];
                    result += distances[from][to];
                }
                ((IntegerPath<T>)path).SetLength(result);
            }
        }

        public override IPath<T> CreatePath(int[] locations)
        {
            return new IntegerPath<T>(this, locations);
        }
    }
}