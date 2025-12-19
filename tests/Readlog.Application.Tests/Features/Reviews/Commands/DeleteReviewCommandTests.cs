using FluentAssertions;
using Moq;
using Readlog.Application.Abstractions;
using Readlog.Application.Features.Reviews.Commands;
using Readlog.Domain.Abstractions;
using Readlog.Domain.Entities;

namespace Readlog.Application.Tests.Features.Reviews.Commands;

public class DeleteReviewCommandTests
{
    private readonly Mock<IReviewRepository> _reviewRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeleteReviewCommandHandler _handler;

    public DeleteReviewCommandTests()
    {
        _reviewRepositoryMock = new Mock<IReviewRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new DeleteReviewCommandHandler(
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
        var review = Review.Create(Guid.NewGuid(), 3, "Title", "Content");
        SetCreatedBy(review, userId);

        _reviewRepositoryMock
            .Setup(r => r.GetByIdAsync(reviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var command = new DeleteReviewCommand(reviewId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithNonExistingReview_ShouldReturnFailure()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        _reviewRepositoryMock
            .Setup(r => r.GetByIdAsync(reviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Review?)null);

        var command = new DeleteReviewCommand(reviewId);

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
        var review = Review.Create(Guid.NewGuid(), 3, "Title", "Content");
        SetCreatedBy(review, Guid.NewGuid());

        _reviewRepositoryMock
            .Setup(r => r.GetByIdAsync(reviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(Guid.NewGuid());

        var command = new DeleteReviewCommand(reviewId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Unauthorized");
    }

    [Fact]
    public async Task Handle_WithOwnReview_ShouldCallRepositoryDelete()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var review = Review.Create(Guid.NewGuid(), 3, "Title", "Content");
        SetCreatedBy(review, userId);

        _reviewRepositoryMock
            .Setup(r => r.GetByIdAsync(reviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var command = new DeleteReviewCommand(reviewId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _reviewRepositoryMock.Verify(r => r.Delete(review), Times.Once);
    }

    private static void SetCreatedBy(Review review, Guid userId)
    {
        var property = typeof(Review).GetProperty("CreatedBy");
        property?.SetValue(review, userId);
    }
}
