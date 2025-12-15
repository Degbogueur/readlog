using Microsoft.EntityFrameworkCore;
using Readlog.Domain.Abstractions;
using Readlog.Domain.Entities;
using Readlog.Infrastructure.Data;

namespace Readlog.Infrastructure.Repositories;

public class ReadingListRepository(
    ApplicationDbContext dbContext) : IReadingListRepository
{
    public async Task<ReadingList?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.ReadingLists
            .Include(rl => rl.Items)
            .FirstOrDefaultAsync(rl => rl.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ReadingList>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.ReadingLists
            .Include(rl => rl.Items)
            .Where(rl => rl.CreatedBy == userId)
            .OrderByDescending(rl => rl.CreatedAt)
            .ToListAsync(cancellationToken);
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