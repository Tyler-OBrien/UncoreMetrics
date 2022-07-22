using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Okolni.Source.Query;
using Okolni.Source.Query.Source;

namespace Shared_Collectors.Helpers
{
    public class QueryPoolItem<T>
    {
        public QueryPoolItem(IQueryConnectionPool queryConnectionPool, T item)
        {
            QueryConnectionPool = queryConnectionPool;
            Item = item;
        }

        public IQueryConnectionPool QueryConnectionPool { get; set; }

        public T Item { get; set; }
    }
}
