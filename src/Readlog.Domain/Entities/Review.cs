using Readlog.Domain.Abstractions;
using Readlog.Domain.Events;
using Readlog.Domain.Exceptions;
using Readlog.Domain.ValueObjects;

namespace Readlog.Domain.Entities;

public sealed class Review : AggregateRoot, IAuditable, ISoftDeletable
{
    public Guid BookId { get; private set; }
    public Rating Rating { get; private set; } = null!;
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    private Review() { }

    public static Review Create(Guid bookId, int rating, string title, string content)
    {
        if (bookId == Guid.Empty)
            throw new DomainException("BookId cannot be empty.");

        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Review title cannot be empty.");

        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException("Review content cannot be empty.");

        var review = new Review
        {
            BookId = bookId,
            Rating = Rating.Create(rating),
            Title = title.Trim(),
            Content = content.Trim()
        };

        review.AddDomainEvent(new ReviewPostedEvent(review.Id, review.BookId, review.Rating.Value));

        return review;
    }

    public void Update(int rating, string title, string content)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Review title cannot be empty.");

        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException("Review content cannot be empty.");

        Rating = Rating.Create(rating);
        Title = title.Trim();
        Content = content.Trim();
    }
}
