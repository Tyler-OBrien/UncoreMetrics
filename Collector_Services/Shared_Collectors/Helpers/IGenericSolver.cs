namespace Shared_Collectors.Helpers;

public interface IGenericSolver<TIn, TOut>
{
    public TOut? Solve(TIn item);
}