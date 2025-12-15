using Readlog.Domain.Abstractions;
using Readlog.Domain.Enums;
using Readlog.Domain.Exceptions;

namespace Readlog.Domain.Entities;

public sealed class ReadingListItem : BaseEntity
{
    public Guid BookId { get; private set; }
    public ReadingStatus Status { get; private set; }
    public DateTime AddedAt { get; private set; }

    private ReadingListItem() { }

    internal static ReadingListItem Create(Guid bookId, ReadingStatus status = ReadingStatus.WantToRead)
    {
        if (bookId == Guid.Empty)
            throw new DomainException("BookId cannot be empty.");

        return new ReadingListItem
        {
            BookId = bookId,
            Status = status,
            AddedAt = DateTime.UtcNow
        };
    }

    internal void UpdateStatus(ReadingStatus newStatus)
    {
        Status = newStatus;
    }
}
