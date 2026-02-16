using Sharpen;
using System.Collections.Generic;

namespace TheTravelingSalesman.Impl
{
    public class DoublePath<T> : AbstractPath<T>, IPath<T>
    {
        private double length = double.NaN;

        protected internal DoublePath(IProblem<T> problem, int[] locations)
            : base(problem, locations)
        {
        }

        protected internal DoublePath(IProblem<T> problem)
            : base(problem, new int[0])
        {
            this.length = 0;
        }

        protected internal DoublePath(IProblem<T> problem, int start)
            : base(problem, new int[] { start })
        {
        }

        protected internal DoublePath(IProblem<T> problem, int[] locations, int nextLocation)
            : base(problem, Sharpen.Arrays.CopyOf(locations, locations.Length + 1))
        {
            this.locations[this.locations.Length - 1] = nextLocation;
        }

        public override IPath<T> To(int nextLocation)
        {
            return new DoublePath<T>(problem, locations, nextLocation);
        }

        public virtual double GetLength()
        {
            if (double.IsNaN(length))
            {
                problem.CalculateLengths(System.Linq.Enumerable.ToList<IPath<T>>(new IPath<T>[] { this }));
            }
            return length;
        }

        protected internal virtual void SetLength(double length)
        {
            this.length = length;
        }

        public override string ToString()
        {
            IList<T> originalLocations = problem.GetLocations(locations);
            return "Path [locations=[" + string.Join(", ", originalLocations) + "], length=" + length + "]";
        }

        /* (non-Javadoc)
		* @see de.fxworld.thetravelingsalesman.impl.IPath#compareTo(de.fxworld.thetravelingsalesman.impl.Path)
		*/

        public override int CompareTo(IPath<T> o)
        {
            return JavaMath.Compare(GetLength(), ((DoublePath<T>)o).GetLength());
        }

        public override bool IsBetter(IPath<T> globalBestPath)
        {
            bool result = true;
            if (globalBestPath != null && ((DoublePath<T>)globalBestPath).GetLength() < GetLength())
            {
                result = false;
            }
            return result;
        }

        public override double GetPathLength()
        {
            return length / 1000.0;
        }
    }
}