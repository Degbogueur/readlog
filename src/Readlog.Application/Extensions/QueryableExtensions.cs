using System.Linq.Expressions;

namespace Readlog.Application.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> ApplyPagination<T>(
        this IQueryable<T> query,
        int page,
        int pageSize)
    {
        return query
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
    }

    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> query,
        string? sortBy,
        bool sortDescending,
        Dictionary<string, Expression<Func<T, object>>> sortMappings,
        Expression<Func<T, object>> defaultSort)
    {
        if (string.IsNullOrWhiteSpace(sortBy) || !sortMappings.TryGetValue(sortBy.ToLower(), out var sortExpression))
        {
            sortExpression = defaultSort;
        }

        return sortDescending
            ? query.OrderByDescending(sortExpression)
            : query.OrderBy(sortExpression);
    }
}
