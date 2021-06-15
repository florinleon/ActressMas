using Sharpen;
using System.Collections.Generic;

namespace TheTravelingSalesman
{
    public class NearestNeighborSolver<T> : ISolver<T>
    {
        private IProblem<T> problem;

        public NearestNeighborSolver(IProblem<T> problem)
        {
            this.problem = problem;
        }

        public virtual IPath<T> Solve()
        {
            IPath<T> start = problem.CreatePath();
            for (int j = 0; j < problem.GetLocationsCount(); j++)
            {
                IList<IPath<T>> neighbors = new List<IPath<T>>();
                for (int i = 0; i < problem.GetLocationsCount(); i++)
                {
                    if (!start.Contains(i))
                    {
                        neighbors.Add(start.To(i));
                    }
                }
                problem.CalculateLengths(neighbors);
                neighbors.Sort();
                start = neighbors[0];
            }
            return start;
        }

        public virtual IProblem<T> GetProblem()
        {
            return problem;
        }

        public override string ToString()
        {
            return "NearestNeighborSolver []";
        }
    }
}