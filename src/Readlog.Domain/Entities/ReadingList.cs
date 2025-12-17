using Readlog.Domain.Abstractions;
using Readlog.Domain.Enums;
using Readlog.Domain.Events;
using Readlog.Domain.Exceptions;

namespace Readlog.Domain.Entities;

public sealed class ReadingList : AggregateRoot, IAuditable, ISoftDeletable
{
    private readonly List<ReadingListItem> _items = [];

    public string Name { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
    public IReadOnlyCollection<ReadingListItem> Items => _items.AsReadOnly();

    private ReadingList() { }

    public static ReadingList Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Reading list name cannot be empty.");

        return new ReadingList
        {
            Name = name.Trim()
        };
    }

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new DomainException("Reading list name cannot be empty.");

        Name = newName.Trim();
    }

    public void AddBook(Guid bookId, ReadingStatus status = ReadingStatus.WantToRead)
    {
        if (_items.Any(i => i.BookId == bookId))
            throw new DomainException("This book is already in the reading list.");

        _items.Add(ReadingListItem.Create(bookId, status));
        AddDomainEvent(new BookAddedToListEvent(Id, bookId));
    }

    public void RemoveBook(Guid bookId)
    {
        var item = _items.FirstOrDefault(i => i.BookId == bookId)
            ?? throw new DomainException("Book not found in this reading list.");

        _items.Remove(item);
    }

    public void UpdateBookStatus(Guid bookId, ReadingStatus newStatus)
    {
        var item = _items.FirstOrDefault(i => i.BookId == bookId)
            ?? throw new DomainException("Book not found in this reading list.");

        item.UpdateStatus(newStatus);
    }
}
