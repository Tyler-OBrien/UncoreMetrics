using System.Collections.Concurrent;

namespace Steam_Collector.Helpers;

public class AsyncResolveQueue<TIn, TOut> : IDisposable
{
    private readonly IGenericAsyncSolver<TIn, TOut> _solvingMethod;

    private readonly CancellationToken _token;
    private int _completed;
    private int _failed;
    private readonly ConcurrentQueue<TIn> _incoming = new();
    private readonly int _incomingItems;
    private int _running;
    private int _successful;
    private bool _beingDisposed;


    public AsyncResolveQueue(IEnumerable<TIn> items, int workerCount, IGenericAsyncSolver<TIn, TOut> solver,
        CancellationToken token)
    {
        _token = token;

        _solvingMethod = solver;

        _incoming = new ConcurrentQueue<TIn>(items);
        Interlocked.Add(ref _incomingItems, items.Count());
        for (var i = 0; i < workerCount; i++)
            Consume(token).ContinueWith(t => Console.WriteLine(t.Exception),
                TaskContinuationOptions.OnlyOnFaulted).ContinueWith(
                t => Console.WriteLine($"Worker exited safely {t.Status}"),
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
    }


    private async Task Consume(CancellationToken token)
    {
        try
        {
            while (_incoming.TryDequeue(out var item))
            {
                if (token.IsCancellationRequested || _beingDisposed)
                    break;
                try
                {
                    Interlocked.Increment(ref _running);
                    var outItem = await _solvingMethod.Solve(item);
                    if (outItem.success == false || outItem.item == null)
                    {
                        Interlocked.Increment(ref _failed);
                    }
                    else
                    {
                        Interlocked.Increment(ref _successful);
                    }
                    if (outItem.item != null)
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
            Console.WriteLine($"Exception in Consume of Thread {Environment.CurrentManagedThreadId}");
            Console.WriteLine(ex);
        }
    }

    public TIn? Peek()
    {
        if (_incoming.TryPeek(out var result)) return result;

        return default;
    }
}