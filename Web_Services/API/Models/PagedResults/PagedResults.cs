using Microsoft.EntityFrameworkCore;

namespace UncoreMetrics.API.Models.PagedResults;

public class PagedResult<T>
{
    public int CurrentPage { get; set; }
    public int PageCount { get; set; }
    public int PageSize { get; set; }
    public int RowCount { get; set; }
    public List<T> Results { get; set; } = new();
}

public static class PageResultExtensions
{
    public static async Task<PagedResult<T>> GetPaged<T>(this IQueryable<T> query,
        int page, int pageSize, CancellationToken token) where T : class
    {
        var result = new PagedResult<T>();
        result.CurrentPage = page;
        result.PageSize = pageSize;
        result.RowCount = await query.AsNoTracking().CountAsync(token);


        var pageCount = (double)result.RowCount / pageSize;
        result.PageCount = (int)Math.Ceiling(pageCount);

        var skip = (page - 1) * pageSize;
        result.Results = await query.AsNoTracking().Skip(skip).Take(pageSize).ToListAsync(token);

        return result;
    }
}