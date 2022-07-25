namespace Shared_Collectors.Helpers;

public interface IGenericAsyncSolver<TIn, TOut>
{
    public Task<TOut?> Solve(TIn item);
}