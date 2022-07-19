using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared_Collectors.Helpers
{
    public class ResolveQueue<TIn, TOut> : IDisposable
    {
        BlockingCollection<TIn> _incoming = new BlockingCollection<TIn>();

        public ConcurrentBag<TOut> Outgoing { get; private set; } = new ConcurrentBag<TOut>();

        private readonly IGenericSolver<TIn, TOut> _solvingMethod;
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


        public ResolveQueue(IEnumerable<TIn> items, int workerCount, IGenericSolver<TIn, TOut> solver)
        {

            _solvingMethod = solver;

            _incoming = new BlockingCollection<TIn>(new ConcurrentQueue<TIn>(items));
            Interlocked.Add(ref _incomingItems, items.Count());
            for (int i = 0; i < workerCount; i++)
            {
                Task.Factory.StartNew(Consume);
            }
        }


        public ResolveQueue(int workerCount, IGenericSolver<TIn, TOut> solver)
        {
            for (int i = 0; i < workerCount; i++)
            {
                Task.Factory.StartNew(Consume);
            }

            _solvingMethod = solver;
        }

        public void Add(TIn incoming)
        {
            _incoming.Add(incoming);
            Interlocked.Increment(ref _incomingItems);
        }



        private void Consume()
        {
            foreach (var item in _incoming.GetConsumingEnumerable())
            {
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
            _incoming.CompleteAdding();
        }


    }
}
