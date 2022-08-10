namespace UncoreMetrics.Steam_Collector.Helpers;

public interface IGenericAsyncSolver<TIn, TOut>
{
    public Task<(TOut? item, bool success)> Solve(TIn item);
}