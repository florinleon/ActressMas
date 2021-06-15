namespace TheTravelingSalesman
{
    public abstract class AbstractPath<T> : IPath<T>
    {
        protected internal IProblem<T> problem;

        protected internal int[] locations;

        public AbstractPath(IProblem<T> problem, int[] locations)
        {
            this.problem = problem;
            this.locations = locations;
        }

        public virtual IProblem<T> GetProblem()
        {
            return problem;
        }

        public virtual int[] GetLocations()
        {
            return locations;
        }

        public virtual int GetLast()
        {
            if (locations.Length > 0)
            {
                return locations[locations.Length - 1];
            }
            else
            {
                return -1;
            }
        }

        public virtual int GetLocationsCount()
        {
            return locations.Length;
        }

        public virtual bool Contains(int location)
        {
            bool result = false;
            for (int i = 0; i < locations.Length; i++)
            {
                if (locations[i] == location)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        public abstract int CompareTo(IPath<T> arg1);

        public abstract bool IsBetter(IPath<T> arg1);

        public abstract IPath<T> To(int arg1);

        public abstract double GetPathLength();
    }
}