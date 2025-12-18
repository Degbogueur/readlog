using FluentAssertions;
using Readlog.Domain.Entities;
using Readlog.Domain.Enums;
using Readlog.Domain.Events;
using Readlog.Domain.Exceptions;

namespace Readlog.Domain.Tests.Entities;

public class ReadingListTests
{
    [Fact]
    public void Create_WithValidName_ShouldCreateReadingList()
    {
        // Act
        var readingList = ReadingList.Create("My Reading List");

        // Assert
        readingList.Should().NotBeNull();
        readingList.Id.Should().NotBeEmpty();
        readingList.Name.Should().Be("My Reading List");
        readingList.Items.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithWhitespace_ShouldTrimName()
    {
        // Act
        var readingList = ReadingList.Create("  My List  ");

        // Assert
        readingList.Name.Should().Be("My List");
    }

    [Fact]
    public void Create_WithNullName_ShouldThrowDomainException()
    {
        // Act
        var act = () => ReadingList.Create(null!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Reading list name cannot be empty.");
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowDomainException()
    {
        // Act
        var act = () => ReadingList.Create(string.Empty);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Reading list name cannot be empty.");
    }

    [Fact]
    public void Create_WithWhitespaceOnlyName_ShouldThrowDomainException()
    {
        // Act
        var act = () => ReadingList.Create("   ");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Reading list name cannot be empty.");
    }

    [Fact]
    public void Rename_WithValidName_ShouldUpdateName()
    {
        // Arrange
        var readingList = ReadingList.Create("Old Name");

        // Act
        readingList.Rename("New Name");

        // Assert
        readingList.Name.Should().Be("New Name");
    }

    [Fact]
    public void Rename_WithWhitespace_ShouldTrimName()
    {
        // Arrange
        var readingList = ReadingList.Create("Old Name");

        // Act
        readingList.Rename("  New Name  ");

        // Assert
        readingList.Name.Should().Be("New Name");
    }

    [Fact]
    public void Rename_WithNullName_ShouldThrowDomainException()
    {
        // Arrange
        var readingList = ReadingList.Create("Name");

        // Act
        var act = () => readingList.Rename(null!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Reading list name cannot be empty.");
    }

    [Fact]
    public void Rename_WithEmptyName_ShouldThrowDomainException()
    {
        // Arrange
        var readingList = ReadingList.Create("Name");

        // Act
        var act = () => readingList.Rename(string.Empty);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Reading list name cannot be empty.");
    }

    [Fact]
    public void AddBook_WithValidBookId_ShouldAddItem()
    {
        // Arrange
        var readingList = ReadingList.Create("My List");
        var bookId = Guid.NewGuid();

        // Act
        readingList.AddBook(bookId);

        // Assert
        readingList.Items.Should().HaveCount(1);
        readingList.Items.First().BookId.Should().Be(bookId);
        readingList.Items.First().Status.Should().Be(ReadingStatus.WantToRead);
    }

    [Fact]
    public void AddBook_WithCustomStatus_ShouldAddItemWithStatus()
    {
        // Arrange
        var readingList = ReadingList.Create("My List");
        var bookId = Guid.NewGuid();

        // Act
        readingList.AddBook(bookId, ReadingStatus.Reading);

        // Assert
        readingList.Items.First().Status.Should().Be(ReadingStatus.Reading);
    }

    [Fact]
    public void AddBook_ShouldRaiseDomainEvent()
    {
        // Arrange
        var readingList = ReadingList.Create("My List");
        var bookId = Guid.NewGuid();

        // Act
        readingList.AddBook(bookId);

        // Assert
        readingList.DomainEvents.Should().HaveCount(1);
        readingList.DomainEvents.First().Should().BeOfType<BookAddedToListEvent>();

        var domainEvent = (BookAddedToListEvent)readingList.DomainEvents.First();
        domainEvent.ReadingListId.Should().Be(readingList.Id);
        domainEvent.BookId.Should().Be(bookId);
    }

    [Fact]
    public void AddBook_MultipleTimes_ShouldAddMultipleItems()
    {
        // Arrange
        var readingList = ReadingList.Create("My List");
        var bookId1 = Guid.NewGuid();
        var bookId2 = Guid.NewGuid();

        // Act
        readingList.AddBook(bookId1);
        readingList.AddBook(bookId2);

        // Assert
        readingList.Items.Should().HaveCount(2);
    }

    [Fact]
    public void AddBook_WithDuplicateBookId_ShouldThrowConflictException()
    {
        // Arrange
        var readingList = ReadingList.Create("My List");
        var bookId = Guid.NewGuid();
        readingList.AddBook(bookId);

        // Act
        var act = () => readingList.AddBook(bookId);

        // Assert
        act.Should().Throw<ConflictException>()
            .WithMessage("This book is already in the reading list.");
    }

    [Fact]
    public void RemoveBook_WithExistingBookId_ShouldRemoveItem()
    {
        // Arrange
        var readingList = ReadingList.Create("My List");
        var bookId = Guid.NewGuid();
        readingList.AddBook(bookId);

        // Act
        readingList.RemoveBook(bookId);

        // Assert
        readingList.Items.Should().BeEmpty();
    }

    [Fact]
    public void RemoveBook_ShouldOnlyRemoveSpecifiedBook()
    {
        // Arrange
        var readingList = ReadingList.Create("My List");
        var bookId1 = Guid.NewGuid();
        var bookId2 = Guid.NewGuid();
        readingList.AddBook(bookId1);
        readingList.AddBook(bookId2);

        // Act
        readingList.RemoveBook(bookId1);

        // Assert
        readingList.Items.Should().HaveCount(1);
        readingList.Items.First().BookId.Should().Be(bookId2);
    }

    [Fact]
    public void RemoveBook_WithNonExistingBookId_ShouldThrowNotFoundException()
    {
        // Arrange
        var readingList = ReadingList.Create("My List");
        var bookId = Guid.NewGuid();

        // Act
        var act = () => readingList.RemoveBook(bookId);

        // Assert
        act.Should().Throw<NotFoundException>()
            .WithMessage("Book not found in this reading list.");
    }

    [Fact]
    public void UpdateBookStatus_WithValidBookId_ShouldUpdateStatus()
    {
        // Arrange
        var readingList = ReadingList.Create("My List");
        var bookId = Guid.NewGuid();
        readingList.AddBook(bookId, ReadingStatus.WantToRead);

        // Act
        readingList.UpdateBookStatus(bookId, ReadingStatus.Finished);

        // Assert
        readingList.Items.First().Status.Should().Be(ReadingStatus.Finished);
    }

    [Theory]
    [InlineData(ReadingStatus.WantToRead)]
    [InlineData(ReadingStatus.Reading)]
    [InlineData(ReadingStatus.Finished)]
    [InlineData(ReadingStatus.Abandoned)]
    public void UpdateBookStatus_WithAllStatuses_ShouldUpdateCorrectly(ReadingStatus status)
    {
        // Arrange
        var readingList = ReadingList.Create("My List");
        var bookId = Guid.NewGuid();
        readingList.AddBook(bookId);

        // Act
        readingList.UpdateBookStatus(bookId, status);

        // Assert
        readingList.Items.First().Status.Should().Be(status);
    }

    [Fact]
    public void UpdateBookStatus_WithNonExistingBookId_ShouldThrowNotFoundException()
    {
        // Arrange
        var readingList = ReadingList.Create("My List");
        var bookId = Guid.NewGuid();

        // Act
        var act = () => readingList.UpdateBookStatus(bookId, ReadingStatus.Finished);

        // Assert
        act.Should().Throw<NotFoundException>()
            .WithMessage("Book not found in this reading list.");
    }
}
