using FluentAssertions;
using Readlog.Application.Features.ReadingLists.Commands;

namespace Readlog.Application.Tests.Features.ReadingLists.Validators;

public class CreateReadingListCommandValidatorTests
{
    private readonly CreateReadingListCommandValidator _validator;

    public CreateReadingListCommandValidatorTests()
    {
        _validator = new CreateReadingListCommandValidator();
    }

    [Fact]
    public void Validate_WithValidName_ShouldPass()
    {
        // Arrange
        var command = new CreateReadingListCommand("My Reading List");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldFail()
    {
        // Arrange
        var command = new CreateReadingListCommand(string.Empty);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_WithNameExceeding100Characters_ShouldFail()
    {
        // Arrange
        var longName = new string('a', 101);
        var command = new CreateReadingListCommand(longName);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Name" &&
            e.ErrorMessage.Contains("100"));
    }

    [Fact]
    public void Validate_WithNameExactly100Characters_ShouldPass()
    {
        // Arrange
        var name = new string('a', 100);
        var command = new CreateReadingListCommand(name);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithWhitespaceOnlyName_ShouldFail()
    {
        // Arrange
        var command = new CreateReadingListCommand("   ");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
