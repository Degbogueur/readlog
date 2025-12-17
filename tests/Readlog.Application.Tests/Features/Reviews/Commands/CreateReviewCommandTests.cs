using FluentAssertions;
using Moq;
using Readlog.Application.Abstractions;
using Readlog.Application.Features.Reviews.Commands;
using Readlog.Domain.Abstractions;
using Readlog.Domain.Entities;

namespace Readlog.Application.Tests.Features.Reviews.Commands;

public class CreateReviewCommandTests
{
    private readonly Mock<IReviewRepository> _reviewRepositoryMock;
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateReviewCommandHandler _handler;

    public CreateReviewCommandTests()
    {
        _reviewRepositoryMock = new Mock<IReviewRepository>();
        _bookRepositoryMock = new Mock<IBookRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreateReviewCommandHandler(
            _reviewRepositoryMock.Object,
            _bookRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var book = Book.Create("Title", "Author");

        _bookRepositoryMock
            .Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
        _reviewRepositoryMock
            .Setup(r => r.GetByBookAndUserAsync(bookId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Review?)null);

        var command = new CreateReviewCommand(bookId, 5, "Great!", "Loved it!");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Rating.Should().Be(5);
        result.Value.Title.Should().Be("Great!");
        result.Value.Content.Should().Be("Loved it!");
    }

    [Fact]
    public async Task Handle_WithNonExistingBook_ShouldReturnFailure()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        _bookRepositoryMock
            .Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        var command = new CreateReviewCommand(bookId, 5, "Great!", "Loved it!");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("NotFound");
    }

    [Fact]
    public async Task Handle_WithExistingReviewByUser_ShouldReturnConflict()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var book = Book.Create("Title", "Author");
        var existingReview = Review.Create(bookId, 3, "Old Review", "Old Content");

        _bookRepositoryMock
            .Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
        _reviewRepositoryMock
            .Setup(r => r.GetByBookAndUserAsync(bookId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingReview);

        var command = new CreateReviewCommand(bookId, 5, "New Review", "New Content");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Conflict");
        result.Error.Message.Should().Contain("already reviewed");
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryAddAsync()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var book = Book.Create("Title", "Author");

        _bookRepositoryMock
            .Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
        _reviewRepositoryMock
            .Setup(r => r.GetByBookAndUserAsync(bookId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Review?)null);

        var command = new CreateReviewCommand(bookId, 5, "Great!", "Loved it!");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _reviewRepositoryMock.Verify(
            r => r.AddAsync(
                It.Is<Review>(review => review.Rating.Value == 5 && review.Title == "Great!"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithConflict_ShouldNotCallSaveChanges()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var book = Book.Create("Title", "Author");
        var existingReview = Review.Create(bookId, 3, "Old", "Content");

        _bookRepositoryMock
            .Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
        _reviewRepositoryMock
            .Setup(r => r.GetByBookAndUserAsync(bookId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingReview);

        var command = new CreateReviewCommand(bookId, 5, "New", "Content");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
