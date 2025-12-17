using FluentAssertions;
using Readlog.Domain.Exceptions;
using Readlog.Domain.ValueObjects;

namespace Readlog.Domain.Tests.ValueObjects;

public class ISBNTests
{
    [Theory]
    [InlineData("0-306-40615-2")]      // ISBN-10 with dashes
    [InlineData("0306406152")]          // ISBN-10 without dashes
    [InlineData("978-3-16-148410-0")]   // ISBN-13 with dashes
    [InlineData("9783161484100")]       // ISBN-13 without dashes
    [InlineData("0 306 40615 2")]       // ISBN-10 with spaces
    public void Create_WithValidISBN_ShouldReturnISBN(string value)
    {
        // Act
        var isbn = ISBN.Create(value);

        // Assert
        isbn.Should().NotBeNull();
        isbn.Value.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Create_WithValidISBN10_ShouldRemoveDashes()
    {
        // Arrange
        var value = "0-306-40615-2";

        // Act
        var isbn = ISBN.Create(value);

        // Assert
        isbn.Value.Should().Be("0306406152");
    }

    [Fact]
    public void Create_WithValidISBN13_ShouldRemoveDashes()
    {
        // Arrange
        var value = "978-3-16-148410-0";

        // Act
        var isbn = ISBN.Create(value);

        // Assert
        isbn.Value.Should().Be("9783161484100");
    }

    [Fact]
    public void Create_WithISBN10EndingWithX_ShouldBeValid()
    {
        // Arrange - ISBN-10 avec X comme chiffre de contrôle
        var value = "0-8044-2957-X";

        // Act
        var isbn = ISBN.Create(value);

        // Assert
        isbn.Should().NotBeNull();
        isbn.Value.Should().Be("080442957X");
    }

    [Fact]
    public void Create_WithNull_ShouldThrowValidationException()
    {
        // Act
        var act = () => ISBN.Create(null!);

        // Assert
        act.Should().Throw<ValidationException>()
            .WithMessage("ISBN is required.");
    }

    [Fact]
    public void Create_WithEmptyString_ShouldThrowValidationException()
    {
        // Act
        var act = () => ISBN.Create(string.Empty);

        // Assert
        act.Should().Throw<ValidationException>()
            .WithMessage("ISBN is required.");
    }

    [Fact]
    public void Create_WithWhitespace_ShouldThrowValidationException()
    {
        // Act
        var act = () => ISBN.Create("   ");

        // Assert
        act.Should().Throw<ValidationException>()
            .WithMessage("ISBN is required.");
    }

    [Theory]
    [InlineData("123")]                  // Too short
    [InlineData("12345678901234")]       // Too long (14 digits)
    [InlineData("abcdefghij")]           // Letters instead of numbers
    [InlineData("0-306-40615-9")]        // Invalid checksum for ISBN-10
    [InlineData("978-3-16-148410-5")]    // Invalid checksum for ISBN-13
    public void Create_WithInvalidISBN_ShouldThrowValidationException(string value)
    {
        // Act
        var act = () => ISBN.Create(value);

        // Assert
        act.Should().Throw<ValidationException>()
            .WithMessage($"'{value}' is not a valid ISBN.");
    }

    [Fact]
    public void CreateOrDefault_WithValidISBN_ShouldReturnISBN()
    {
        // Arrange
        var value = "978-3-16-148410-0";

        // Act
        var isbn = ISBN.CreateOrDefault(value);

        // Assert
        isbn.Should().NotBeNull();
        isbn!.Value.Should().Be("9783161484100");
    }

    [Fact]
    public void CreateOrDefault_WithNull_ShouldReturnNull()
    {
        // Act
        var isbn = ISBN.CreateOrDefault(null);

        // Assert
        isbn.Should().BeNull();
    }

    [Fact]
    public void CreateOrDefault_WithEmptyString_ShouldReturnNull()
    {
        // Act
        var isbn = ISBN.CreateOrDefault(string.Empty);

        // Assert
        isbn.Should().BeNull();
    }

    [Fact]
    public void CreateOrDefault_WithInvalidISBN_ShouldReturnNull()
    {
        // Act
        var isbn = ISBN.CreateOrDefault("invalid-isbn");

        // Assert
        isbn.Should().BeNull();
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        var isbn1 = ISBN.Create("978-3-16-148410-0");
        var isbn2 = ISBN.Create("9783161484100");

        // Act & Assert
        isbn1.Should().Be(isbn2);
        (isbn1 == isbn2).Should().BeTrue();
        (isbn1 != isbn2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        var isbn1 = ISBN.Create("978-3-16-148410-0");
        var isbn2 = ISBN.Create("0-306-40615-2");

        // Act & Assert
        isbn1.Should().NotBe(isbn2);
        (isbn1 == isbn2).Should().BeFalse();
        (isbn1 != isbn2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var isbn = ISBN.Create("978-3-16-148410-0");

        // Act & Assert
        isbn.Equals(null).Should().BeFalse();
        (isbn == null).Should().BeFalse();
        (null == isbn).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldReturnSameHashCode()
    {
        // Arrange
        var isbn1 = ISBN.Create("978-3-16-148410-0");
        var isbn2 = ISBN.Create("9783161484100");

        // Act & Assert
        isbn1.GetHashCode().Should().Be(isbn2.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var isbn = ISBN.Create("978-3-16-148410-0");

        // Act & Assert
        isbn.ToString().Should().Be("9783161484100");
    }
}
