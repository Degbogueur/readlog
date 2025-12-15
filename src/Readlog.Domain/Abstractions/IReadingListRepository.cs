using Readlog.Domain.Entities;

namespace Readlog.Domain.Abstractions;

public interface IReadingListRepository
{
    Task<ReadingList?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReadingList>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(ReadingList readingList, CancellationToken cancellationToken = default);
    void Update(ReadingList readingList);
    void Delete(ReadingList readingList);
}
