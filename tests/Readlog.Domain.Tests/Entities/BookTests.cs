using FluentAssertions;
using Readlog.Domain.Entities;
using Readlog.Domain.Events;
using Readlog.Domain.Exceptions;

namespace Readlog.Domain.Tests.Entities;

public class BookTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateBook()
    {
        // Arrange
        var title = "The Lord of the Rings";
        var author = "J.R.R. Tolkien";

        // Act
        var book = Book.Create(title, author);

        // Assert
        book.Should().NotBeNull();
        book.Id.Should().NotBeEmpty();
        book.Title.Should().Be(title);
        book.Author.Should().Be(author);
        book.Isbn.Should().BeNull();
        book.Description.Should().BeNull();
        book.PublishedDate.Should().BeNull();
    }

    [Fact]
    public void Create_WithAllOptionalFields_ShouldCreateBook()
    {
        // Arrange
        var title = "Clean Code";
        var author = "Robert C. Martin";
        var isbn = "978-0-13-235088-4";
        var description = "A handbook of agile software craftsmanship";
        var publishedDate = new DateOnly(2008, 8, 1);

        // Act
        var book = Book.Create(title, author, isbn, description, publishedDate);

        // Assert
        book.Title.Should().Be(title);
        book.Author.Should().Be(author);
        book.Isbn.Should().NotBeNull();
        book.Isbn!.Value.Should().Be("9780132350884");
        book.Description.Should().Be(description);
        book.PublishedDate.Should().Be(publishedDate);
    }

    [Fact]
    public void Create_WithWhitespaceInTitleAndAuthor_ShouldTrimValues()
    {
        // Arrange
        var title = "  The Hobbit  ";
        var author = "  J.R.R. Tolkien  ";

        // Act
        var book = Book.Create(title, author);

        // Assert
        book.Title.Should().Be("The Hobbit");
        book.Author.Should().Be("J.R.R. Tolkien");
    }

    [Fact]
    public void Create_WithInvalidISBN_ShouldCreateBookWithNullISBN()
    {
        // Arrange
        var invalidIsbn = "invalid-isbn";

        // Act
        var book = Book.Create("Title", "Author", invalidIsbn);

        // Assert
        book.Isbn.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldRaiseDomainEvent()
    {
        // Act
        var book = Book.Create("Title", "Author");

        // Assert
        book.DomainEvents.Should().HaveCount(1);
        book.DomainEvents.First().Should().BeOfType<BookCreatedEvent>();

        var domainEvent = (BookCreatedEvent)book.DomainEvents.First();
        domainEvent.BookId.Should().Be(book.Id);
        domainEvent.Title.Should().Be("Title");
        domainEvent.Author.Should().Be("Author");
    }

    [Fact]
    public void Create_WithNullTitle_ShouldThrowDomainException()
    {
        // Act
        var act = () => Book.Create(null!, "Author");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Book title cannot be empty.");
    }

    [Fact]
    public void Create_WithEmptyTitle_ShouldThrowDomainException()
    {
        // Act
        var act = () => Book.Create(string.Empty, "Author");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Book title cannot be empty.");
    }

    [Fact]
    public void Create_WithWhitespaceOnlyTitle_ShouldThrowDomainException()
    {
        // Act
        var act = () => Book.Create("   ", "Author");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Book title cannot be empty.");
    }

    [Fact]
    public void Create_WithNullAuthor_ShouldThrowDomainException()
    {
        // Act
        var act = () => Book.Create("Title", null!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Author name cannot be empty.");
    }

    [Fact]
    public void Create_WithEmptyAuthor_ShouldThrowDomainException()
    {
        // Act
        var act = () => Book.Create("Title", string.Empty);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Author name cannot be empty.");
    }

    [Fact]
    public void Create_WithWhitespaceOnlyAuthor_ShouldThrowDomainException()
    {
        // Act
        var act = () => Book.Create("Title", "   ");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Author name cannot be empty.");
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateBook()
    {
        // Arrange
        var book = Book.Create("Old Title", "Old Author");
        var newTitle = "New Title";
        var newAuthor = "New Author";
        var newDescription = "New Description";
        var newIsbn = "978-3-16-148410-0";
        var newPublishedDate = new DateOnly(2023, 1, 1);

        // Act
        book.Update(newTitle, newAuthor, newIsbn, newDescription, newPublishedDate);

        // Assert
        book.Title.Should().Be(newTitle);
        book.Author.Should().Be(newAuthor);
        book.Isbn.Should().NotBeNull();
        book.Description.Should().Be(newDescription);
        book.PublishedDate.Should().Be(newPublishedDate);
    }

    [Fact]
    public void Update_WithWhitespace_ShouldTrimValues()
    {
        // Arrange
        var book = Book.Create("Old Title", "Old Author");

        // Act
        book.Update("  New Title  ", "  New Author  ", null, "  Description  ");

        // Assert
        book.Title.Should().Be("New Title");
        book.Author.Should().Be("New Author");
        book.Description.Should().Be("Description");
    }

    [Fact]
    public void Update_ShouldNotRaiseDomainEvent()
    {
        // Arrange
        var book = Book.Create("Title", "Author");
        book.ClearDomainEvents();

        // Act
        book.Update("New Title", "New Author");

        // Assert
        book.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Update_WithNullTitle_ShouldThrowDomainException()
    {
        // Arrange
        var book = Book.Create("Title", "Author");

        // Act
        var act = () => book.Update(null!, "New Author");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Book title cannot be empty.");
    }

    [Fact]
    public void Update_WithEmptyTitle_ShouldThrowDomainException()
    {
        // Arrange
        var book = Book.Create("Title", "Author");

        // Act
        var act = () => book.Update(string.Empty, "New Author");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Book title cannot be empty.");
    }

    [Fact]
    public void Update_WithNullAuthor_ShouldThrowDomainException()
    {
        // Arrange
        var book = Book.Create("Title", "Author");

        // Act
        var act = () => book.Update("New Title", null!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Author name cannot be empty.");
    }

    [Fact]
    public void Update_WithEmptyAuthor_ShouldThrowDomainException()
    {
        // Arrange
        var book = Book.Create("Title", "Author");

        // Act
        var act = () => book.Update("New Title", string.Empty);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Author name cannot be empty.");
    }

    [Fact]
    public void NewBook_ShouldHaveIsDeletedFalse()
    {
        // Act
        var book = Book.Create("Title", "Author");

        // Assert
        book.IsDeleted.Should().BeFalse();
        book.DeletedAt.Should().BeNull();
        book.DeletedBy.Should().BeNull();
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        // Arrange
        var book = Book.Create("Title", "Author");
        book.DomainEvents.Should().NotBeEmpty();

        // Act
        book.ClearDomainEvents();

        // Assert
        book.DomainEvents.Should().BeEmpty();
    }
}
