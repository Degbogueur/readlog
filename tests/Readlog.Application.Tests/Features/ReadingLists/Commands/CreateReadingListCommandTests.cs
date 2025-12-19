using FluentAssertions;
using Moq;
using Readlog.Application.Abstractions;
using Readlog.Application.Features.ReadingLists.Commands;
using Readlog.Domain.Abstractions;
using Readlog.Domain.Entities;

namespace Readlog.Application.Tests.Features.ReadingLists.Commands;

public class CreateReadingListCommandTests
{
    private readonly Mock<IReadingListRepository> _readingListRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateReadingListCommandHandler _handler;

    public CreateReadingListCommandTests()
    {
        _readingListRepositoryMock = new Mock<IReadingListRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreateReadingListCommandHandler(
            _readingListRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidName_ShouldReturnSuccess()
    {
        // Arrange
        var command = new CreateReadingListCommand("My Reading List");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("My Reading List");
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryAddAsync()
    {
        // Arrange
        var command = new CreateReadingListCommand("My List");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _readingListRepositoryMock.Verify(
            r => r.AddAsync(
                It.Is<ReadingList>(rl => rl.Name == "My List"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChangesAsync()
    {
        // Arrange
        var command = new CreateReadingListCommand("My List");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnNewId()
    {
        // Arrange
        var command = new CreateReadingListCommand("My List");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Value.Id.Should().NotBeEmpty();
    }
}
