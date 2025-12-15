using Microsoft.EntityFrameworkCore;
using Readlog.Domain.Abstractions;
using Readlog.Domain.Entities;
using Readlog.Infrastructure.Data;

namespace Readlog.Infrastructure.Repositories;

public class ReviewRepository(
    ApplicationDbContext dbContext) : IReviewRepository
{
    public async Task<Review?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Reviews
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Review>> GetByBookIdAsync(Guid bookId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Reviews
            .Where(r => r.BookId == bookId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
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
