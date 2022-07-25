using System.Collections.Concurrent;

namespace Shared_Collectors.Helpers;

public class AsyncResolveQueue<TIn, TOut> : IDisposable
{
    private readonly IGenericAsyncSolver<TIn, TOut> _solvingMethod;

    private readonly CancellationTokenSource _tokenSource;
    private int _completed;
    private int _failed;
    private readonly ConcurrentQueue<TIn> _incoming = new();
    private readonly int _incomingItems;
    private int _running;
    private int _successful;


    public AsyncResolveQueue(IEnumerable<TIn> items, int workerCount, IGenericAsyncSolver<TIn, TOut> solver)
    {
        _tokenSource = new CancellationTokenSource();


        _solvingMethod = solver;

        _incoming = new ConcurrentQueue<TIn>(items);
        Interlocked.Add(ref _incomingItems, items.Count());
        for (var i = 0; i < workerCount; i++)
        {
            Consume(_tokenSource.Token).ContinueWith(t => Console.WriteLine(t.Exception),
                TaskContinuationOptions.OnlyOnFaulted).ContinueWith(
                t => Console.WriteLine($"Worker exited safely {t.Status}"),
                TaskContinuationOptions.OnlyOnRanToCompletion);
            ;
        }
    }

    public AsyncResolveQueue(IEnumerable<TIn> items, int workerCount, IGenericAsyncSolver<TIn, TOut> solver,
        CancellationToken token)
    {
        _tokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);

        _solvingMethod = solver;

        _incoming = new ConcurrentQueue<TIn>(items);
        Interlocked.Add(ref _incomingItems, items.Count());
        for (var i = 0; i < workerCount; i++)
            Consume(_tokenSource.Token).ContinueWith(t => Console.WriteLine(t.Exception),
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
        _tokenSource.Dispose();
    }


    private async Task Consume(CancellationToken token)
    {
        try
        {
            while (_incoming.TryDequeue(out var item))
            {
                if (token.IsCancellationRequested)
                    break;
                try
                {
                    Interlocked.Increment(ref _running);
                    var outItem = await _solvingMethod.Solve(item);
                    if (outItem == null)
                    {
                        Interlocked.Increment(ref _failed);
                    }
                    else
                    {
                        Interlocked.Increment(ref _successful);
                        Outgoing.Add(outItem);
                    }
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
            Console.WriteLine($"Exception in Consume of Thread {Thread.CurrentThread.ManagedThreadId}");
            Console.WriteLine(ex);
        }
    }

    public TIn? Peek()
    {
        if (_incoming.TryPeek(out var result)) return result;

        return default;
    }
}