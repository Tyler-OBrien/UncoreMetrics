using Microsoft.Extensions.Logging.Abstractions;
using UncoreMetrics.Data.Helpers;

namespace UncoreMetrics.Tests;

public class AsyncResolveQueueTests
{
    private static async Task WaitUntilDone<TIn, TOut>(AsyncResolveQueue<TIn, TOut> queue)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while (!queue.Done)
            await Task.Delay(10, timeout.Token);
    }

    [Fact]
    public async Task AllItemsSucceed()
    {
        var solver = new DelegateSolver(i => Task.FromResult<(string?, bool)>((i.ToString(), true)));

        using var queue = new AsyncResolveQueue<int, string>(NullLogger.Instance, Enumerable.Range(0, 50), 4,
            solver, TestContext.Current.CancellationToken);
        await WaitUntilDone(queue);

        Assert.Equal(50, queue.Completed);
        Assert.Equal(50, queue.Successful);
        Assert.Equal(0, queue.Failed);
        Assert.Equal(Enumerable.Range(0, 50).Select(i => i.ToString()).Order(), queue.Outgoing.Order());
    }

    [Fact]
    public async Task FailedItemsAreCountedAndNullResultsDropped()
    {
        var solver = new DelegateSolver(i =>
            Task.FromResult<(string?, bool)>(i % 2 == 0 ? (i.ToString(), true) : (null, false)));

        using var queue = new AsyncResolveQueue<int, string>(NullLogger.Instance, Enumerable.Range(0, 10), 3,
            solver, TestContext.Current.CancellationToken);
        await WaitUntilDone(queue);

        Assert.Equal(5, queue.Successful);
        Assert.Equal(5, queue.Failed);
        Assert.Equal(5, queue.Outgoing.Count);
    }

    [Fact]
    public async Task ThrowingSolver_StillReachesDone()
    {
        var solver = new DelegateSolver(i =>
            i % 3 == 0
                ? throw new InvalidOperationException("boom")
                : Task.FromResult<(string?, bool)>((i.ToString(), true)));

        // A single worker would previously die on the first exception and never finish the queue
        using var queue = new AsyncResolveQueue<int, string>(NullLogger.Instance, Enumerable.Range(0, 9), 1,
            solver, TestContext.Current.CancellationToken);
        await WaitUntilDone(queue);

        Assert.Equal(9, queue.Completed);
        Assert.Equal(3, queue.Failed);
        Assert.Equal(6, queue.Successful);
    }

    [Fact]
    public async Task CancelledToken_StopsProcessing()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var solver = new DelegateSolver(i => Task.FromResult<(string?, bool)>((i.ToString(), true)));

        using var queue = new AsyncResolveQueue<int, string>(NullLogger.Instance, Enumerable.Range(0, 10), 2,
            solver, cts.Token);
        await Task.Delay(50, TestContext.Current.CancellationToken);

        Assert.Equal(0, queue.Completed);
        Assert.False(queue.Done);
        Assert.Equal(10, queue.QueueCount);
    }

    private class DelegateSolver(Func<int, Task<(string? item, bool success)>> solve)
        : IGenericAsyncSolver<int, string>
    {
        public Task<(string? item, bool success)> Solve(int item) => solve(item);
    }
}
