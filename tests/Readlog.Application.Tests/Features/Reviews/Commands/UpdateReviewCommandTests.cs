using FluentAssertions;
using Moq;
using Readlog.Application.Abstractions;
using Readlog.Application.Features.Reviews.Commands;
using Readlog.Domain.Abstractions;
using Readlog.Domain.Entities;

namespace Readlog.Application.Tests.Features.Reviews.Commands;

public class UpdateReviewCommandTests
{
    private readonly Mock<IReviewRepository> _reviewRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateReviewCommandHandler _handler;

    public UpdateReviewCommandTests()
    {
        _reviewRepositoryMock = new Mock<IReviewRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new UpdateReviewCommandHandler(
            _reviewRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithOwnReview_ShouldReturnSuccess()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var review = Review.Create(Guid.NewGuid(), 3, "Old Title", "Old Content");
        SetCreatedBy(review, userId);

        _reviewRepositoryMock
            .Setup(r => r.GetByIdAsync(reviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var command = new UpdateReviewCommand(reviewId, 5, "New Title", "New Content");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Rating.Should().Be(5);
        result.Value.Title.Should().Be("New Title");
    }

    [Fact]
    public async Task Handle_WithNonExistingReview_ShouldReturnFailure()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        _reviewRepositoryMock
            .Setup(r => r.GetByIdAsync(reviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Review?)null);

        var command = new UpdateReviewCommand(reviewId, 5, "Title", "Content");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("NotFound");
    }

    [Fact]
    public async Task Handle_WithOtherUsersReview_ShouldReturnUnauthorized()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var review = Review.Create(Guid.NewGuid(), 3, "Title", "Content");
        SetCreatedBy(review, ownerId);

        _reviewRepositoryMock
            .Setup(r => r.GetByIdAsync(reviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        var command = new UpdateReviewCommand(reviewId, 5, "New Title", "New Content");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Unauthorized");
    }

    [Fact]
    public async Task Handle_WithUnauthorized_ShouldNotCallSaveChanges()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var review = Review.Create(Guid.NewGuid(), 3, "Title", "Content");
        SetCreatedBy(review, Guid.NewGuid());

        _reviewRepositoryMock
            .Setup(r => r.GetByIdAsync(reviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(Guid.NewGuid());

        var command = new UpdateReviewCommand(reviewId, 5, "Title", "Content");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static void SetCreatedBy(Review review, Guid userId)
    {
        var property = typeof(Review).GetProperty("CreatedBy");
        property?.SetValue(review, userId);
    }
}
