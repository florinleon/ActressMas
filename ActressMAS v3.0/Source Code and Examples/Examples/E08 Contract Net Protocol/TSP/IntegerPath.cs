using Sharpen;
using System.Collections.Generic;

namespace TheTravelingSalesman.Impl
{
    public class IntegerPath<T> : AbstractPath<T>, IPath<T>
    {
        private int length = int.MinValue;

        protected internal IntegerPath(IProblem<T> problem, int[] locations)
            : base(problem, locations)
        {
        }

        protected internal IntegerPath(IProblem<T> problem)
            : base(problem, new int[0])
        {
            this.length = 0;
        }

        protected internal IntegerPath(IProblem<T> problem, int start)
            : base(problem, new int[] { start })
        {
        }

        protected internal IntegerPath(IProblem<T> problem, int[] locations, int nextLocation)
            : base(problem, Sharpen.Arrays.CopyOf(locations, locations.Length + 1))
        {
            this.locations[this.locations.Length - 1] = nextLocation;
        }

        public override IPath<T> To(int nextLocation)
        {
            return new IntegerPath<T>(problem, locations, nextLocation);
        }

        public virtual double GetLength()
        {
            if (length == int.MinValue)
            {
                IList<IPath<T>> list = new List<IPath<T>>(1);
                list.Add(this);

                problem.CalculateLengths(list);
            }
            return length;
        }

        public virtual void SetLength(int length)
        {
            this.length = length;
        }

        public override string ToString()
        {
            IList<T> originalLocations = problem.GetLocations(locations);
            return "Path [locations=" + string.Join(", ", originalLocations) + ", length=" + length + "]";
        }

        /* (non-Javadoc)
		* @see de.fxworld.thetravelingsalesman.impl.IPath#compareTo(de.fxworld.thetravelingsalesman.impl.Path)
		*/

        public override int CompareTo(IPath<T> o)
        {
            return JavaMath.Compare(GetLength(), ((IntegerPath<T>)o).GetLength());
        }

        public override bool IsBetter(IPath<T> globalBestPath)
        {
            bool result = true;
            if (globalBestPath != null && ((IntegerPath<T>)globalBestPath).GetLength() < GetLength())
            {
                result = false;
            }
            return result;
        }

        public override double GetPathLength()
        {
            return (double)length / 1000.0;
        }
    }
}