using System.Collections.Concurrent;

namespace UncoreMetrics.Steam_Collector.Helpers;

public class ResolveQueue<TIn, TOut> : IDisposable
{
    private readonly BlockingCollection<TIn> _incoming = new();
    private readonly IGenericSolver<TIn, TOut> _solvingMethod;
    private int _completed;
    private int _failed;
    private int _incomingItems;
    private int _running;
    private int _successful;


    public ResolveQueue(IEnumerable<TIn> items, int workerCount, IGenericSolver<TIn, TOut> solver)
    {
        _solvingMethod = solver;

        _incoming = new BlockingCollection<TIn>(new ConcurrentQueue<TIn>(items));
        Interlocked.Add(ref _incomingItems, items.Count());
        for (var i = 0; i < workerCount; i++) Task.Factory.StartNew(Consume);
    }


    public ResolveQueue(int workerCount, IGenericSolver<TIn, TOut> solver)
    {
        for (var i = 0; i < workerCount; i++) Task.Factory.StartNew(Consume);

        _solvingMethod = solver;
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
        _incoming.CompleteAdding();
    }

    public void Add(TIn incoming)
    {
        _incoming.Add(incoming);
        Interlocked.Increment(ref _incomingItems);
    }


    private void Consume()
    {
        foreach (var item in _incoming.GetConsumingEnumerable())
            try
            {
                Interlocked.Increment(ref _running);
                var outItem = _solvingMethod.Solve(item);
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