using MediatR;

namespace Readlog.Domain.Abstractions;

public interface IDomainEvent : INotification
{
    DateTime OccuredOn { get; }
}
