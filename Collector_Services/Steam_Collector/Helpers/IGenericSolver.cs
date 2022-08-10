namespace UncoreMetrics.Steam_Collector.Helpers;

public interface IGenericSolver<TIn, TOut>
{
    public TOut? Solve(TIn item);
}