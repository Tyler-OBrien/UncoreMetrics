using Okolni.Source.Query.Source;

namespace UncoreMetrics.Steam_Collector.Helpers;

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