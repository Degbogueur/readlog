using FluentAssertions;
using Readlog.Domain.Entities;
using Readlog.Domain.Events;
using Readlog.Domain.Exceptions;

namespace Readlog.Domain.Tests.Entities;

public class ReviewTests
{
    private readonly Guid _validBookId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldCreateReview()
    {
        // Act
        var review = Review.Create(_validBookId, 4, "Great book!", "Loved every chapter.");

        // Assert
        review.Should().NotBeNull();
        review.Id.Should().NotBeEmpty();
        review.BookId.Should().Be(_validBookId);
        review.Rating.Value.Should().Be(4);
        review.Title.Should().Be("Great book!");
        review.Content.Should().Be("Loved every chapter.");
    }

    [Fact]
    public void Create_WithWhitespace_ShouldTrimValues()
    {
        // Act
        var review = Review.Create(_validBookId, 5, "  Title  ", "  Content  ");

        // Assert
        review.Title.Should().Be("Title");
        review.Content.Should().Be("Content");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void Create_WithValidRating_ShouldCreateReview(int rating)
    {
        // Act
        var review = Review.Create(_validBookId, rating, "Title", "Content");

        // Assert
        review.Rating.Value.Should().Be(rating);
    }

    [Fact]
    public void Create_ShouldRaiseDomainEvent()
    {
        // Act
        var review = Review.Create(_validBookId, 5, "Amazing!", "Best book ever.");

        // Assert
        review.DomainEvents.Should().HaveCount(1);
        review.DomainEvents.First().Should().BeOfType<ReviewPostedEvent>();

        var domainEvent = (ReviewPostedEvent)review.DomainEvents.First();
        domainEvent.ReviewId.Should().Be(review.Id);
        domainEvent.BookId.Should().Be(_validBookId);
        domainEvent.Rating.Should().Be(5);
    }

    [Fact]
    public void Create_WithEmptyBookId_ShouldThrowDomainException()
    {
        // Act
        var act = () => Review.Create(Guid.Empty, 4, "Title", "Content");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("BookId cannot be empty.");
    }

    [Fact]
    public void Create_WithNullTitle_ShouldThrowDomainException()
    {
        // Act
        var act = () => Review.Create(_validBookId, 4, null!, "Content");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Review title cannot be empty.");
    }

    [Fact]
    public void Create_WithEmptyTitle_ShouldThrowDomainException()
    {
        // Act
        var act = () => Review.Create(_validBookId, 4, string.Empty, "Content");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Review title cannot be empty.");
    }

    [Fact]
    public void Create_WithWhitespaceOnlyTitle_ShouldThrowDomainException()
    {
        // Act
        var act = () => Review.Create(_validBookId, 4, "   ", "Content");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Review title cannot be empty.");
    }

    [Fact]
    public void Create_WithNullContent_ShouldThrowDomainException()
    {
        // Act
        var act = () => Review.Create(_validBookId, 4, "Title", null!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Review content cannot be empty.");
    }

    [Fact]
    public void Create_WithEmptyContent_ShouldThrowDomainException()
    {
        // Act
        var act = () => Review.Create(_validBookId, 4, "Title", string.Empty);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Review content cannot be empty.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(6)]
    [InlineData(100)]
    public void Create_WithInvalidRating_ShouldThrowValidationException(int rating)
    {
        // Act
        var act = () => Review.Create(_validBookId, rating, "Title", "Content");

        // Assert
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateReview()
    {
        // Arrange
        var review = Review.Create(_validBookId, 3, "Old Title", "Old Content");

        // Act
        review.Update(5, "New Title", "New Content");

        // Assert
        review.Rating.Value.Should().Be(5);
        review.Title.Should().Be("New Title");
        review.Content.Should().Be("New Content");
    }

    [Fact]
    public void Update_WithWhitespace_ShouldTrimValues()
    {
        // Arrange
        var review = Review.Create(_validBookId, 3, "Title", "Content");

        // Act
        review.Update(4, "  New Title  ", "  New Content  ");

        // Assert
        review.Title.Should().Be("New Title");
        review.Content.Should().Be("New Content");
    }

    [Fact]
    public void Update_ShouldNotRaiseDomainEvent()
    {
        // Arrange
        var review = Review.Create(_validBookId, 3, "Title", "Content");
        review.ClearDomainEvents();

        // Act
        review.Update(5, "Updated", "Updated Content");

        // Assert
        review.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Update_WithNullTitle_ShouldThrowDomainException()
    {
        // Arrange
        var review = Review.Create(_validBookId, 4, "Title", "Content");

        // Act
        var act = () => review.Update(5, null!, "Content");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Review title cannot be empty.");
    }

    [Fact]
    public void Update_WithEmptyContent_ShouldThrowDomainException()
    {
        // Arrange
        var review = Review.Create(_validBookId, 4, "Title", "Content");

        // Act
        var act = () => review.Update(5, "Title", string.Empty);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Review content cannot be empty.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Update_WithInvalidRating_ShouldThrowValidationException(int rating)
    {
        // Arrange
        var review = Review.Create(_validBookId, 4, "Title", "Content");

        // Act
        var act = () => review.Update(rating, "Title", "Content");

        // Assert
        act.Should().Throw<ValidationException>();
    }
}
