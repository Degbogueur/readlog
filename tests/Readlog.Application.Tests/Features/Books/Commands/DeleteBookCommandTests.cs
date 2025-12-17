using FluentAssertions;
using Moq;
using Readlog.Application.Abstractions;
using Readlog.Application.Features.Books.Commands;
using Readlog.Domain.Abstractions;
using Readlog.Domain.Entities;

namespace Readlog.Application.Tests.Features.Books.Commands;

public class DeleteBookCommandTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeleteBookCommandHandler _handler;

    public DeleteBookCommandTests()
    {
        _bookRepositoryMock = new Mock<IBookRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new DeleteBookCommandHandler(_bookRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingBook_ShouldReturnSuccess()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var existingBook = Book.Create("Title", "Author");

        _bookRepositoryMock
            .Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBook);

        var command = new DeleteBookCommand(bookId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithNonExistingBook_ShouldReturnFailure()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        _bookRepositoryMock
            .Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        var command = new DeleteBookCommand(bookId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("NotFound");
    }

    [Fact]
    public async Task Handle_WithExistingBook_ShouldCallRepositoryDelete()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var existingBook = Book.Create("Title", "Author");

        _bookRepositoryMock
            .Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBook);

        var command = new DeleteBookCommand(bookId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _bookRepositoryMock.Verify(r => r.Delete(existingBook), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingBook_ShouldCallSaveChangesAsync()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var existingBook = Book.Create("Title", "Author");

        _bookRepositoryMock
            .Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBook);

        var command = new DeleteBookCommand(bookId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistingBook_ShouldNotCallDelete()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        _bookRepositoryMock
            .Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        var command = new DeleteBookCommand(bookId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _bookRepositoryMock.Verify(r => r.Delete(It.IsAny<Book>()), Times.Never);
    }
}
