using Readlog.Domain.Entities;

namespace Readlog.Domain.Abstractions;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Review>> GetByBookIdAsync(Guid bookId, CancellationToken cancellationToken = default);
    Task<Review?> GetByBookAndUserAsync(Guid bookId, Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Review review, CancellationToken cancellationToken = default);
    void Update(Review review);
    void Delete(Review review);
}
