using FluentAssertions;
using Moq;
using Readlog.Application.Abstractions;
using Readlog.Application.Features.ReadingLists.Commands;
using Readlog.Domain.Abstractions;
using Readlog.Domain.Entities;
using Readlog.Domain.Enums;

namespace Readlog.Application.Tests.Features.ReadingLists.Commands;

public class AddBookToReadingListCommandTests
{
    private readonly Mock<IReadingListRepository> _readingListRepositoryMock;
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly AddBookToReadingListCommandHandler _handler;

    public AddBookToReadingListCommandTests()
    {
        _readingListRepositoryMock = new Mock<IReadingListRepository>();
        _bookRepositoryMock = new Mock<IBookRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new AddBookToReadingListCommandHandler(
            _readingListRepositoryMock.Object,
            _bookRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldReturnSuccessWithReadingList()
    {
        // Arrange
        var readingListId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var readingList = ReadingList.Create("My List");
        SetCreatedBy(readingList, userId);
        var book = Book.Create("Title", "Author");

        _readingListRepositoryMock
            .Setup(r => r.GetByIdAsync(readingListId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(readingList);
        _bookRepositoryMock
            .Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var command = new AddBookToReadingListCommand(readingListId, bookId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items[0].BookId.Should().Be(bookId);
    }

    [Fact]
    public async Task Handle_WithNonExistingReadingList_ShouldReturnFailure()
    {
        // Arrange
        var readingListId = Guid.NewGuid();
        _readingListRepositoryMock
            .Setup(r => r.GetByIdAsync(readingListId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReadingList?)null);

        var command = new AddBookToReadingListCommand(readingListId, Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("NotFound");
    }

    [Fact]
    public async Task Handle_WithOtherUsersReadingList_ShouldReturnUnauthorized()
    {
        // Arrange
        var readingListId = Guid.NewGuid();
        var readingList = ReadingList.Create("My List");
        SetCreatedBy(readingList, Guid.NewGuid());

        _readingListRepositoryMock
            .Setup(r => r.GetByIdAsync(readingListId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(readingList);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(Guid.NewGuid());

        var command = new AddBookToReadingListCommand(readingListId, Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Forbidden");
    }

    [Fact]
    public async Task Handle_WithNonExistingBook_ShouldReturnFailure()
    {
        // Arrange
        var readingListId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var readingList = ReadingList.Create("My List");
        SetCreatedBy(readingList, userId);

        _readingListRepositoryMock
            .Setup(r => r.GetByIdAsync(readingListId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(readingList);
        _bookRepositoryMock
            .Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var command = new AddBookToReadingListCommand(readingListId, bookId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("NotFound");
    }

    [Fact]
    public async Task Handle_WithCustomStatus_ShouldAddBookWithStatus()
    {
        // Arrange
        var readingListId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var readingList = ReadingList.Create("My List");
        SetCreatedBy(readingList, userId);
        var book = Book.Create("Title", "Author");

        _readingListRepositoryMock
            .Setup(r => r.GetByIdAsync(readingListId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(readingList);
        _bookRepositoryMock
            .Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var command = new AddBookToReadingListCommand(readingListId, bookId, ReadingStatus.Reading);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        readingList.Items.First().Status.Should().Be(ReadingStatus.Reading);
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChangesAsync()
    {
        // Arrange
        var readingListId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var readingList = ReadingList.Create("My List");
        SetCreatedBy(readingList, userId);
        var book = Book.Create("Title", "Author");

        _readingListRepositoryMock
            .Setup(r => r.GetByIdAsync(readingListId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(readingList);
        _bookRepositoryMock
            .Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var command = new AddBookToReadingListCommand(readingListId, bookId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryUpdate()
    {
        // Arrange
        var readingListId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var readingList = ReadingList.Create("My List");
        SetCreatedBy(readingList, userId);
        var book = Book.Create("Title", "Author");

        _readingListRepositoryMock
            .Setup(r => r.GetByIdAsync(readingListId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(readingList);
        _bookRepositoryMock
            .Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var command = new AddBookToReadingListCommand(readingListId, bookId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _readingListRepositoryMock.Verify(r => r.Update(It.IsAny<ReadingList>()), Times.Once);
    }

    private static void SetCreatedBy(ReadingList readingList, Guid userId)
    {
        var property = typeof(ReadingList).GetProperty("CreatedBy");
        property?.SetValue(readingList, userId);
    }
}
