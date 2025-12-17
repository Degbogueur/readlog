using FluentAssertions;
using Moq;
using Readlog.Application.Abstractions;
using Readlog.Application.Features.Books.Commands;
using Readlog.Domain.Abstractions;
using Readlog.Domain.Entities;

namespace Readlog.Application.Tests.Features.Books.Commands;

public class CreateBookCommandTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateBookCommandHandler _handler;

    public CreateBookCommandTests()
    {
        _bookRepositoryMock = new Mock<IBookRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreateBookCommandHandler(_bookRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccessWithBookResponse()
    {
        // Arrange
        var command = new CreateBookCommand(
            "Clean Code",
            "Robert C. Martin",
            "978-0-13-235088-4",
            "A handbook of agile software craftsmanship",
            new DateOnly(2008, 8, 1));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Title.Should().Be("Clean Code");
        result.Value.Author.Should().Be("Robert C. Martin");
        result.Value.Isbn.Should().Be("9780132350884");
        result.Value.Description.Should().Be("A handbook of agile software craftsmanship");
        result.Value.PublishedDate.Should().Be(new DateOnly(2008, 8, 1));
    }

    [Fact]
    public async Task Handle_WithMinimalData_ShouldReturnSuccessWithBookResponse()
    {
        // Arrange
        var command = new CreateBookCommand("Title", "Author");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("Title");
        result.Value.Author.Should().Be("Author");
        result.Value.Isbn.Should().BeNull();
        result.Value.Description.Should().BeNull();
        result.Value.PublishedDate.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryAddAsync()
    {
        // Arrange
        var command = new CreateBookCommand("Title", "Author");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _bookRepositoryMock.Verify(
            r => r.AddAsync(
                It.Is<Book>(b => b.Title == "Title" && b.Author == "Author"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallUnitOfWorkSaveChangesAsync()
    {
        // Arrange
        var command = new CreateBookCommand("Title", "Author");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidIsbn_ShouldReturnSuccessWithNullIsbn()
    {
        // Arrange
        var command = new CreateBookCommand("Title", "Author", "invalid-isbn");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Isbn.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnNewGuidAsId()
    {
        // Arrange
        var command = new CreateBookCommand("Title", "Author");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Value.Id.Should().NotBeEmpty();
    }
}
