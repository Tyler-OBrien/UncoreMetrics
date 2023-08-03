using System.Collections.Concurrent;

namespace Ping_Collector_Probe.Models.Resolvers;

public class AsyncResolveQueue<TIn, TOut> : IDisposable
{
    private readonly ConcurrentQueue<TIn> _incoming = new();
    private readonly int _incomingItems;
    private readonly ILogger _logger;
    private readonly IGenericAsyncSolver<TIn, TOut> _solvingMethod;

    private readonly CancellationToken _token;
    private bool _beingDisposed;
    private int _completed;
    private int _failed;
    private int _running;
    private int _successful;


    public AsyncResolveQueue(ILogger logger, IEnumerable<TIn> items, int workerCount,
        IGenericAsyncSolver<TIn, TOut> solver,
        CancellationToken token)
    {
        _logger = logger;
        _token = token;
        _solvingMethod = solver;

        _incoming = new ConcurrentQueue<TIn>(items);
        Interlocked.Add(ref _incomingItems, items.Count());
        for (var i = 0; i < workerCount; i++)
            Consume(token).ContinueWith(t => _logger.LogError(t.Exception, "Worker Exception"),
                TaskContinuationOptions.OnlyOnFaulted).ContinueWith(
                t => _logger.LogDebug("Worker exited safely {workerStatus}", t.Status),
                TaskContinuationOptions.OnlyOnRanToCompletion);
    }

    public ConcurrentBag<TOut> Outgoing { get; } = new();

    public int Failed => _failed;

    public int Successful => _successful;

    public int Completed => _completed;

    public int IncomingItems => _incomingItems;

    public int Running => _running;

    public int QueueCount => _incoming.Count;

    public bool Done => _incomingItems == _completed;


    public void Dispose()
    {
        _beingDisposed = true;
        _incoming.Clear();
        Outgoing.Clear();
    }


    private async Task Consume(CancellationToken token)
    {
        try
        {
            if (token.IsCancellationRequested || _beingDisposed)
                return;

            while (_incoming.TryDequeue(out var item))
            {
                if (token.IsCancellationRequested || _beingDisposed)
                    return;
                try
                {
                    Interlocked.Increment(ref _running);
                    var outItem = await _solvingMethod.Solve(item);
                    if (outItem.success == false || outItem.item == null)
                        Interlocked.Increment(ref _failed);
                    else
                        Interlocked.Increment(ref _successful);
                    if (outItem.item != null && !_beingDisposed)
                        Outgoing.Add(outItem.item);
                }
                finally
                {
                    Interlocked.Increment(ref _completed);
                    Interlocked.Decrement(ref _running);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in Consume of Thread {CurrentManagedThreadId}",
                Environment.CurrentManagedThreadId);
        }
    }

    public TIn? Peek()
    {
        if (_incoming.TryPeek(out var result)) return result;

        return default;
    }
}