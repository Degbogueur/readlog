using FluentAssertions;
using Moq;
using Readlog.Application.Features.Books.Queries;
using Readlog.Domain.Abstractions;
using Readlog.Domain.Entities;

namespace Readlog.Application.Tests.Features.Books.Queries;

public class GetAllBooksQueryTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly GetAllBooksQueryHandler _handler;

    public GetAllBooksQueryTests()
    {
        _bookRepositoryMock = new Mock<IBookRepository>();
        _handler = new GetAllBooksQueryHandler(_bookRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedResult()
    {
        // Arrange
        var books = new List<Book>
        {
            Book.Create("Book 1", "Author 1"),
            Book.Create("Book 2", "Author 2")
        };

        _bookRepositoryMock
            .Setup(r => r.GetAllAsync(
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<bool>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((books.AsReadOnly(), 2));

        var query = new GetAllBooksQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WithEmptyResult_ShouldReturnEmptyPagedResult()
    {
        // Arrange
        _bookRepositoryMock
            .Setup(r => r.GetAllAsync(
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<bool>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Book>().AsReadOnly(), 0));

        var query = new GetAllBooksQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldPassParametersToRepository()
    {
        // Arrange
        _bookRepositoryMock
            .Setup(r => r.GetAllAsync(
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<bool>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Book>().AsReadOnly(), 0));

        var query = new GetAllBooksQuery(
            Search: "test",
            SortBy: "title",
            SortDescending: true,
            Page: 2,
            PageSize: 20);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _bookRepositoryMock.Verify(
            r => r.GetAllAsync("test", "title", true, 2, 20, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCalculatePaginationCorrectly()
    {
        // Arrange
        var books = Enumerable.Range(1, 10)
            .Select(i => Book.Create($"Book {i}", $"Author {i}"))
            .ToList();

        _bookRepositoryMock
            .Setup(r => r.GetAllAsync(
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<bool>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((books.AsReadOnly(), 25));

        var query = new GetAllBooksQuery(Page: 1, PageSize: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.TotalCount.Should().Be(25);
        result.Value.TotalPages.Should().Be(3);
        result.Value.HasNextPage.Should().BeTrue();
        result.Value.HasPreviousPage.Should().BeFalse();
    }
}
