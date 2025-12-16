using Readlog.Domain.Abstractions;
using Readlog.Domain.Events;
using Readlog.Domain.Exceptions;
using Readlog.Domain.ValueObjects;

namespace Readlog.Domain.Entities;

public sealed class Book : AggregateRoot, IAuditable, ISoftDeletable
{
    public string Title { get; private set; } = string.Empty;
    public string Author { get; private set; } = string.Empty;
    public ISBN? Isbn { get; private set; }
    public string? Description { get; private set; }
    public DateOnly? PublishedDate { get; private set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    private Book() { }

    public static Book Create(
        string title,
        string author,
        string? isbn = null,
        string? description = null,
        DateOnly? publishedDate = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Book title cannot be empty.");

        if (string.IsNullOrWhiteSpace(author))
            throw new DomainException("Author name cannot be empty.");

        var book = new Book
        {
            Title = title.Trim(),
            Author = author.Trim(),
            Isbn = ISBN.CreateOrDefault(isbn),
            Description = description?.Trim(),
            PublishedDate = publishedDate
        };

        book.AddDomainEvent(new BookCreatedEvent(book.Id, book.Title, book.Author));

        return book;
    }

    public void Update(
        string title,
        string author,
        string? isbn = null,
        string? description = null,
        DateOnly? publishedDate = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Book title cannot be empty.");

        if (string.IsNullOrWhiteSpace(author))
            throw new DomainException("Author name cannot be empty.");

        Title = title.Trim();
        Author = author.Trim();
        Isbn = ISBN.CreateOrDefault(isbn);
        Description = description?.Trim();
        PublishedDate = publishedDate;
    }
}
