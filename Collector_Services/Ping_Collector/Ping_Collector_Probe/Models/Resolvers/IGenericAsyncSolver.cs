namespace Ping_Collector_Probe.Models.Resolvers
{
    public interface IGenericAsyncSolver<TIn, TOut>
    {
        public Task<(TOut? item, bool success)> Solve(TIn item);
    }
}
