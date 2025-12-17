using Readlog.Domain.Entities;

namespace Readlog.Domain.Abstractions;

public interface IReadingListRepository
{
    Task<ReadingList?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<ReadingList> Items, int TotalCount)> GetByUserIdAsync(
        Guid userId, 
        string? search,
        string? sortBy, 
        bool sortDescending, 
        int page, 
        int pageSize, 
        CancellationToken cancellationToken = default);
    Task AddAsync(ReadingList readingList, CancellationToken cancellationToken = default);
    void Update(ReadingList readingList);
    void Delete(ReadingList readingList);
}
