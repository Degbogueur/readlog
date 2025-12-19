using FluentAssertions;
using Moq;
using Readlog.Application.Features.Books.Queries;
using Readlog.Domain.Abstractions;
using Readlog.Domain.Entities;

namespace Readlog.Application.Tests.Features.Books.Queries;

public class GetBookByIdQueryTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly GetBookByIdQueryHandler _handler;

    public GetBookByIdQueryTests()
    {
        _bookRepositoryMock = new Mock<IBookRepository>();
        _handler = new GetBookByIdQueryHandler(_bookRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingBook_ShouldReturnSuccessWithBookResponse()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var book = Book.Create(
            "Clean Code",
            "Robert C. Martin",
            "978-0-13-235088-4",
            "Description",
            new DateOnly(2008, 8, 1));

        _bookRepositoryMock
            .Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        var query = new GetBookByIdQuery(bookId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("Clean Code");
        result.Value.Author.Should().Be("Robert C. Martin");
    }

    [Fact]
    public async Task Handle_WithNonExistingBook_ShouldReturnFailure()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        _bookRepositoryMock
            .Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        var query = new GetBookByIdQuery(bookId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("NotFound");
        result.Error.Message.Should().Contain(bookId.ToString());
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryWithCorrectId()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        _bookRepositoryMock
            .Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        var query = new GetBookByIdQuery(bookId);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _bookRepositoryMock.Verify(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
