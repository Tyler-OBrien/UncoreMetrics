namespace UncoreMetrics.Data.Helpers;

public interface IGenericAsyncSolver<TIn, TOut>
{
    public Task<(TOut? item, bool success)> Solve(TIn item);
}