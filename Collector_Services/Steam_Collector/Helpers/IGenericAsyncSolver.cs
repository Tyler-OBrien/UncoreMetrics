namespace Steam_Collector.Helpers;

public interface IGenericAsyncSolver<TIn, TOut>
{
    public Task<TOut?> Solve(TIn item);
}