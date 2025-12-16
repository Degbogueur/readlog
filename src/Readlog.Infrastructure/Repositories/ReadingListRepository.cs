using Microsoft.EntityFrameworkCore;
using Readlog.Application.Extensions;
using Readlog.Domain.Abstractions;
using Readlog.Domain.Entities;
using Readlog.Infrastructure.Data;
using System.Linq.Expressions;

namespace Readlog.Infrastructure.Repositories;

public class ReadingListRepository(
    ApplicationDbContext dbContext) : IReadingListRepository
{
    private static readonly Dictionary<string, Expression<Func<ReadingList, object>>> SortMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["name"] = rl => rl.Name,
        ["createdAt"] = rl => rl.CreatedAt,
    };

    public async Task<ReadingList?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.ReadingLists
            .Include(rl => rl.Items)
            .FirstOrDefaultAsync(rl => rl.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<ReadingList> Items, int TotalCount)> GetByUserIdAsync(
        Guid userId,
        string? search,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.ReadingLists
            .Include(rl => rl.Items)
            .Where(rl => rl.CreatedBy == userId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(rl => EF.Functions.Like(rl.Name, $"%{search}%"));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = query.ApplySort(
            sortBy,
            sortDescending,
            SortMappings,
            rl => rl.CreatedAt);

        var items = await query
            .ApplyPagination(page, pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(ReadingList readingList, CancellationToken cancellationToken = default)
    {
        await dbContext.ReadingLists.AddAsync(readingList, cancellationToken);
    }

    public void Update(ReadingList readingList)
    {
        dbContext.ReadingLists.Update(readingList);
    }

    public void Delete(ReadingList readingList)
    {
        dbContext.ReadingLists.Remove(readingList);
    }
}