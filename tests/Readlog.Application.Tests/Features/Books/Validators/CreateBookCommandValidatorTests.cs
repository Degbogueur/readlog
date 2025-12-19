using FluentAssertions;
using Readlog.Application.Features.Books.Commands;

namespace Readlog.Application.Tests.Features.Books.Validators;

public class CreateBookCommandValidatorTests
{
    private readonly CreateBookCommandValidator _validator;

    public CreateBookCommandValidatorTests()
    {
        _validator = new CreateBookCommandValidator();
    }

    [Fact]
    public void Validate_WithValidTitle_ShouldPass()
    {
        // Arrange
        var command = new CreateBookCommand("Valid Title", "Author");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyTitle_ShouldFail()
    {
        // Arrange
        var command = new CreateBookCommand(string.Empty, "Author");

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
        var command = new CreateBookCommand(longTitle, "Author");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Title" &&
            e.ErrorMessage.Contains("200"));
    }

    [Fact]
    public void Validate_WithTitleExactly200Characters_ShouldPass()
    {
        // Arrange
        var title = new string('a', 200);
        var command = new CreateBookCommand(title, "Author");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyAuthor_ShouldFail()
    {
        // Arrange
        var command = new CreateBookCommand("Title", string.Empty);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Author");
    }

    [Fact]
    public void Validate_WithAuthorExceeding150Characters_ShouldFail()
    {
        // Arrange
        var longAuthor = new string('a', 151);
        var command = new CreateBookCommand("Title", longAuthor);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Author" &&
            e.ErrorMessage.Contains("150"));
    }

    [Fact]
    public void Validate_WithNullDescription_ShouldPass()
    {
        // Arrange
        var command = new CreateBookCommand("Title", "Author", Description: null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithDescriptionExceeding2000Characters_ShouldFail()
    {
        // Arrange
        var longDescription = new string('a', 2001);
        var command = new CreateBookCommand("Title", "Author", Description: longDescription);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Description" &&
            e.ErrorMessage.Contains("2000"));
    }

    [Fact]
    public void Validate_WithNullPublishedDate_ShouldPass()
    {
        // Arrange
        var command = new CreateBookCommand("Title", "Author", PublishedDate: null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithFuturePublishedDate_ShouldFail()
    {
        // Arrange
        var futureDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var command = new CreateBookCommand("Title", "Author", PublishedDate: futureDate);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "PublishedDate" &&
            e.ErrorMessage.Contains("future"));
    }

    [Fact]
    public void Validate_WithTodayPublishedDate_ShouldPass()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Today);
        var command = new CreateBookCommand("Title", "Author", PublishedDate: today);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithPastPublishedDate_ShouldPass()
    {
        // Arrange
        var pastDate = new DateOnly(2000, 1, 1);
        var command = new CreateBookCommand("Title", "Author", PublishedDate: pastDate);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithMultipleErrors_ShouldStopAtFirstError()
    {
        // Arrange - Both Title and Author are invalid
        var command = new CreateBookCommand(string.Empty, string.Empty);

        // Act
        var result = _validator.Validate(command);

        // Assert - Due to CascadeMode.Stop, should only have one error
        result.Errors.Should().HaveCount(1);
    }
}
