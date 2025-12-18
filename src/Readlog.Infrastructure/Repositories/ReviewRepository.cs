using Microsoft.EntityFrameworkCore;
using Readlog.Application.Extensions;
using Readlog.Domain.Abstractions;
using Readlog.Domain.Entities;
using Readlog.Infrastructure.Data;
using System.Linq.Expressions;

namespace Readlog.Infrastructure.Repositories;

public class ReviewRepository(
    ApplicationDbContext dbContext) : IReviewRepository
{
    private static readonly Dictionary<string, Expression<Func<Review, object>>> SortMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["title"] = r => r.Title,
        ["createdAt"] = r => r.CreatedAt
    };

    public async Task<Review?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Reviews
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<Review> Items, int TotalCount)> GetByBookIdAsync(
        Guid bookId, 
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Reviews
            .Where(r => r.BookId == bookId);

        var totalCount = await query.CountAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(sortBy) && sortBy.Equals("rating", StringComparison.OrdinalIgnoreCase))
        {
            query = sortDescending
                ? query.OrderByDescending(r => EF.Property<int>(r, "Rating"))
                : query.OrderBy(r => EF.Property<int>(r, "Rating"));
        }
        else
        {
            query = query.ApplySort(
                sortBy,
                sortDescending,
                SortMappings,
                r => r.CreatedAt);
        }            

        var items = await query
            .ApplyPagination(page, pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Review?> GetByBookAndUserAsync(Guid bookId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Reviews
            .FirstOrDefaultAsync(r => r.BookId == bookId && r.CreatedBy == userId, cancellationToken);
    }

    public async Task AddAsync(Review review, CancellationToken cancellationToken = default)
    {
        await dbContext.Reviews.AddAsync(review, cancellationToken);
    }

    public void Update(Review review)
    {
        dbContext.Reviews.Update(review);
    }

    public void Delete(Review review)
    {
        dbContext.Reviews.Remove(review);
    }
}
