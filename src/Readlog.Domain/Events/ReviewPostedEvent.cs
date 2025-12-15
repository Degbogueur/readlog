using Readlog.Domain.Abstractions;

namespace Readlog.Domain.Events;

public sealed record ReviewPostedEvent(
    Guid ReviewId,
    Guid BookId,
    int Rating) : IDomainEvent
{
    public DateTime OccuredOn { get; } = DateTime.UtcNow;
}
