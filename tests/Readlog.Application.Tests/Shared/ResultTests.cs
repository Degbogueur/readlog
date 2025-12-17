using FluentAssertions;
using Readlog.Application.Shared;

namespace Readlog.Application.Tests.Shared;

public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_ShouldCreateFailureResult()
    {
        // Arrange
        var error = Error.NotFound("Book", Guid.NewGuid());

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Success_WithError_ShouldThrowException()
    {
        // Act
        var act = () => new TestableResult(true, Error.NotFound("Test", Guid.NewGuid()));

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Success result cannot have an error.");
    }

    [Fact]
    public void Failure_WithNoError_ShouldThrowException()
    {
        // Act
        var act = () => new TestableResult(false, Error.None);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Failure result must have an error.");
    }

    [Fact]
    public void Success_Generic_ShouldCreateSuccessResultWithValue()
    {
        // Arrange
        var value = "test value";

        // Act
        var result = Result.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public void Failure_Generic_ShouldCreateFailureResult()
    {
        // Arrange
        var error = Error.Validation("Invalid input");

        // Act
        var result = Result.Failure<string>(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Value_OnFailure_ShouldThrowException()
    {
        // Arrange
        var result = Result.Failure<string>(Error.Validation("Error"));

        // Act
        var act = () => result.Value;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot access value of a failed result.");
    }

    [Fact]
    public void ImplicitConversion_FromValue_ShouldCreateSuccessResult()
    {
        // Arrange
        string value = "test";

        // Act
        Result<string> result = value;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public void ImplicitConversion_FromNull_ShouldCreateFailureResult()
    {
        // Arrange
        string? value = null;

        // Act
        Result<string> result = value;

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.NullValue);
    }

    [Fact]
    public void Error_NotFound_ShouldCreateCorrectError()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var error = Error.NotFound("Book", id);

        // Assert
        error.Code.Should().Be("Book.NotFound");
        error.Message.Should().Be($"Book with ID '{id}' was not found.");
    }

    [Fact]
    public void Error_Validation_ShouldCreateCorrectError()
    {
        // Act
        var error = Error.Validation("Invalid input");

        // Assert
        error.Code.Should().Be("Validation.Error");
        error.Message.Should().Be("Invalid input");
    }

    [Fact]
    public void Error_Conflict_ShouldCreateCorrectError()
    {
        // Act
        var error = Error.Conflict("Resource already exists");

        // Assert
        error.Code.Should().Be("Conflict.Error");
        error.Message.Should().Be("Resource already exists");
    }

    [Fact]
    public void Error_Unauthorized_ShouldCreateCorrectError()
    {
        // Act
        var error = Error.Unauthorized("Access denied");

        // Assert
        error.Code.Should().Be("Unauthorized.Error");
        error.Message.Should().Be("Access denied");
    }

    [Fact]
    public void Error_Unauthorized_WithDefaultMessage_ShouldUseDefaultMessage()
    {
        // Act
        var error = Error.Unauthorized();

        // Assert
        error.Message.Should().Be("Unauthorized access.");
    }

    private class TestableResult(bool isSuccess, Error error) : Result(isSuccess, error) { }
}
