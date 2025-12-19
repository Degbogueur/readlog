using FluentAssertions;
using Readlog.Application.Features.Reviews.Commands;

namespace Readlog.Application.Tests.Features.Reviews.Validators;

public class CreateReviewCommandValidatorTests
{
    private readonly CreateReviewCommandValidator _validator;

    public CreateReviewCommandValidatorTests()
    {
        _validator = new CreateReviewCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new CreateReviewCommand(Guid.NewGuid(), 5, "Great!", "Loved it!");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyBookId_ShouldFail()
    {
        // Arrange
        var command = new CreateReviewCommand(Guid.Empty, 5, "Title", "Content");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "BookId");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(6)]
    [InlineData(100)]
    public void Validate_WithInvalidRating_ShouldFail(int rating)
    {
        // Arrange
        var command = new CreateReviewCommand(Guid.NewGuid(), rating, "Title", "Content");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Rating");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void Validate_WithValidRating_ShouldPass(int rating)
    {
        // Arrange
        var command = new CreateReviewCommand(Guid.NewGuid(), rating, "Title", "Content");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyTitle_ShouldFail()
    {
        // Arrange
        var command = new CreateReviewCommand(Guid.NewGuid(), 5, string.Empty, "Content");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    public void Validate_WithTitleExceeding200Characters_ShouldFail()
    {
        // Arrange
        var longTitle = new string('a', 201);
        var command = new CreateReviewCommand(Guid.NewGuid(), 5, longTitle, "Content");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    public void Validate_WithEmptyContent_ShouldFail()
    {
        // Arrange
        var command = new CreateReviewCommand(Guid.NewGuid(), 5, "Title", string.Empty);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Content");
    }

    [Fact]
    public void Validate_WithContentExceeding5000Characters_ShouldFail()
    {
        // Arrange
        var longContent = new string('a', 5001);
        var command = new CreateReviewCommand(Guid.NewGuid(), 5, "Title", longContent);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Content");
    }
}
