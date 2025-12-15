namespace Readlog.Domain.Abstractions;

public interface IDomainEvent
{
    DateTime OccuredOn { get; }
}
