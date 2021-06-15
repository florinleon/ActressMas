namespace TheTravelingSalesman
{
    public interface ISolver<T>
    {
        IPath<T> Solve();

        IProblem<T> GetProblem();
    }
}