using Readlog.Domain.Abstractions;

namespace Readlog.Domain.Events;

public sealed record BookAddedToListEvent(
    Guid ReadingListId,
    Guid BookId) : IDomainEvent
{
    public DateTime OccuredOn { get; } = DateTime.UtcNow;
}