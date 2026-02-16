using System;

namespace TheTravelingSalesman
{
    public interface IPath<T> : IComparable<IPath<T>>
    {
        IPath<T> To(int nextLocation);

        IProblem<T> GetProblem();

        int[] GetLocations();

        int GetLast();

        int GetLocationsCount();

        bool Contains(int location);

        bool IsBetter(IPath<T> globalBestPath);

        double GetPathLength();
    }
}