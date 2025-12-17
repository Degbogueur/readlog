using FluentAssertions;
using Moq;
using Readlog.Application.Abstractions;
using Readlog.Application.Features.Books.Commands;
using Readlog.Domain.Abstractions;
using Readlog.Domain.Entities;

namespace Readlog.Application.Tests.Features.Books.Commands;

public class UpdateBookCommandTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateBookCommandHandler _handler;

    public UpdateBookCommandTests()
    {
        _bookRepositoryMock = new Mock<IBookRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new UpdateBookCommandHandler(_bookRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingBook_ShouldReturnSuccessWithUpdatedBook()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var existingBook = Book.Create("Old Title", "Old Author");

        _bookRepositoryMock
            .Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBook);

        var command = new UpdateBookCommand(
            bookId,
            "New Title",
            "New Author",
            "978-3-16-148410-0",
            "New Description",
            new DateOnly(2023, 1, 1));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("New Title");
        result.Value.Author.Should().Be("New Author");
        result.Value.Isbn.Should().Be("9783161484100");
        result.Value.Description.Should().Be("New Description");
    }

    [Fact]
    public async Task Handle_WithNonExistingBook_ShouldReturnFailure()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        _bookRepositoryMock
            .Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        var command = new UpdateBookCommand(bookId, "Title", "Author");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("NotFound");
    }

    [Fact]
    public async Task Handle_WithExistingBook_ShouldCallRepositoryUpdate()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var existingBook = Book.Create("Old Title", "Old Author");

        _bookRepositoryMock
            .Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBook);

        var command = new UpdateBookCommand(bookId, "New Title", "New Author");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _bookRepositoryMock.Verify(r => r.Update(existingBook), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingBook_ShouldCallSaveChangesAsync()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var existingBook = Book.Create("Old Title", "Old Author");

        _bookRepositoryMock
            .Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBook);

        var command = new UpdateBookCommand(bookId, "New Title", "New Author");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistingBook_ShouldNotCallSaveChangesAsync()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        _bookRepositoryMock
            .Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        var command = new UpdateBookCommand(bookId, "Title", "Author");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
