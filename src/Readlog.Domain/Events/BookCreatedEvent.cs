using Readlog.Domain.Abstractions;

namespace Readlog.Domain.Events;

public sealed record BookCreatedEvent(
    Guid BookId,
    string Title,
    string Author) : IDomainEvent
{
    public DateTime OccuredOn { get; } = DateTime.UtcNow;
}
