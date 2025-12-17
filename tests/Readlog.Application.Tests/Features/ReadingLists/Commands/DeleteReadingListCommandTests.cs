using FluentAssertions;
using Moq;
using Readlog.Application.Abstractions;
using Readlog.Application.Features.ReadingLists.Commands;
using Readlog.Domain.Abstractions;
using Readlog.Domain.Entities;

namespace Readlog.Application.Tests.Features.ReadingLists.Commands;

public class DeleteReadingListCommandTests
{
    private readonly Mock<IReadingListRepository> _readingListRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeleteReadingListCommandHandler _handler;

    public DeleteReadingListCommandTests()
    {
        _readingListRepositoryMock = new Mock<IReadingListRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new DeleteReadingListCommandHandler(
            _readingListRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithOwnReadingList_ShouldReturnSuccess()
    {
        // Arrange
        var readingListId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var readingList = ReadingList.Create("My List");
        SetCreatedBy(readingList, userId);

        _readingListRepositoryMock
            .Setup(r => r.GetByIdAsync(readingListId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(readingList);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var command = new DeleteReadingListCommand(readingListId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithNonExistingReadingList_ShouldReturnFailure()
    {
        // Arrange
        var readingListId = Guid.NewGuid();
        _readingListRepositoryMock
            .Setup(r => r.GetByIdAsync(readingListId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReadingList?)null);

        var command = new DeleteReadingListCommand(readingListId);

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

        var command = new DeleteReadingListCommand(readingListId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Unauthorized");
    }

    [Fact]
    public async Task Handle_WithOwnReadingList_ShouldCallRepositoryDelete()
    {
        // Arrange
        var readingListId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var readingList = ReadingList.Create("My List");
        SetCreatedBy(readingList, userId);

        _readingListRepositoryMock
            .Setup(r => r.GetByIdAsync(readingListId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(readingList);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var command = new DeleteReadingListCommand(readingListId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _readingListRepositoryMock.Verify(r => r.Delete(readingList), Times.Once);
    }

    private static void SetCreatedBy(ReadingList readingList, Guid userId)
    {
        var property = typeof(ReadingList).GetProperty("CreatedBy");
        property?.SetValue(readingList, userId);
    }
}
