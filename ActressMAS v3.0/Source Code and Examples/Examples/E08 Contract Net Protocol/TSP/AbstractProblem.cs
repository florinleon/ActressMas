using System;
using System.Collections.Generic;

namespace TheTravelingSalesman.Impl
{
    public abstract class AbstractProblem<L> : IProblem<L>
    {
        private IList<L> locations;

        private volatile IPath<L> bestPath;

        private int locationsCount;

        protected internal int fixedFirstLocation = -1;

        public AbstractProblem()
        {
        }

        public AbstractProblem(IList<L> locations)
            : this(locations, -1)
        {
        }

        public AbstractProblem(IList<L> locations, int fixedFirstLocation)
        {
            this.fixedFirstLocation = fixedFirstLocation;
            SetLocations(locations);
        }

        protected internal virtual void SetLocations(IList<L> locations)
        {
            if (this.locations == null)
            {
                this.locations = Sharpen.Collections.UnmodifiableList(locations);
                this.locationsCount = locations.Count;
            }
            else
            {
                throw new Exception("Locations already set");
            }
        }

        public virtual IList<L> GetLocations()
        {
            return locations;
        }

        public virtual IList<L> GetLocations(int[] intLocations)
        {
            IList<L> result = new List<L>();

            for (int i = 0; i < intLocations.Length; i++)
            {
                result.Add(locations[intLocations[i]]);
            }

            return result;
        }

        public virtual IPath<L> GetBestPath()
        {
            return bestPath;
        }

        public virtual void SetBestPath(IPath<L> bestPath)
        {
            lock (this)
            {
                if (bestPath != null && bestPath.IsBetter(this.bestPath))
                {
                    this.bestPath = bestPath;
                }
            }
        }

        public virtual void ResetBestPath()
        {
            this.bestPath = null;
        }

        public virtual int GetLocationsCount()
        {
            return locationsCount;
        }

        public virtual int GetFixedFirstLocation()
        {
            return fixedFirstLocation;
        }

        public virtual void SetFixedFirstLocation(int fixedFirstLocation)
        {
            this.fixedFirstLocation = fixedFirstLocation;
        }

        public virtual IPath<L> CreatePath()
        {
            return CreatePath(new int[0]);
        }

        public abstract void CalculateLengths(IList<IPath<L>> arg1);

        public abstract IPath<L> CreatePath(int[] arg1);
    }
}