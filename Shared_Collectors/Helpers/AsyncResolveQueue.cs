using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared_Collectors.Helpers
{
    public class AsyncResolveQueue<TIn, TOut> : IDisposable
    {
        ConcurrentQueue<TIn> _incoming = new ConcurrentQueue<TIn>();

        public ConcurrentBag<TOut> Outgoing { get; private set; } = new ConcurrentBag<TOut>();

        private readonly IGenericAsyncSolver<TIn, TOut> _solvingMethod;
        private int _failed;
        private int _successful;
        private int _completed;
        private int _incomingItems;
        private int _running;

        public int Failed
        {
            get => _failed;
        }

        public int Successful
        {
            get => _successful;
        }

        public int Completed
        {
            get => _completed;
        }

        public int IncomingItems
        {
            get => _incomingItems;
        }

        public int Running
        {
            get => _running;
        }

        private readonly CancellationTokenSource _tokenSource;



        public AsyncResolveQueue(IEnumerable<TIn> items, int workerCount, IGenericAsyncSolver<TIn, TOut> solver)
        {
            _tokenSource = new CancellationTokenSource();


            _solvingMethod = solver;

            _incoming = new ConcurrentQueue<TIn>(items);
            Interlocked.Add(ref _incomingItems, items.Count());
            for (int i = 0; i < workerCount; i++)
            {
                Consume(_tokenSource.Token);
            }
        }

        public AsyncResolveQueue(IEnumerable<TIn> items, int workerCount, IGenericAsyncSolver<TIn, TOut> solver, CancellationToken token)
        {
            _tokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);

            _solvingMethod = solver;

            _incoming = new ConcurrentQueue<TIn>(items);
            Interlocked.Add(ref _incomingItems, items.Count());
            for (int i = 0; i < workerCount; i++)
            {
                Consume(_tokenSource.Token).ContinueWith(t => Console.WriteLine(t.Exception),
                    TaskContinuationOptions.OnlyOnFaulted).ContinueWith(t => Console.WriteLine($"Worker exited safely {t.Status}"), TaskContinuationOptions.OnlyOnRanToCompletion);
            }
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

        public int QueueCount
        {
            get
            {
                return _incoming.Count;
            }
        }

        public bool Done
        {
            get
            {
                return _incomingItems == _completed;
            }
        }

        public void Dispose()
        {
            _tokenSource.Dispose();
        }

  
    }
}
