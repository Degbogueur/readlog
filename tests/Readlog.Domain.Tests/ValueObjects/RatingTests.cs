using FluentAssertions;
using Readlog.Domain.Exceptions;
using Readlog.Domain.ValueObjects;

namespace Readlog.Domain.Tests.ValueObjects;

public class RatingTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void Create_WithValidValue_ShouldReturnRating(int value)
    {
        // Act
        var rating = Rating.Create(value);

        // Assert
        rating.Should().NotBeNull();
        rating.Value.Should().Be(value);
    }

    [Fact]
    public void Create_WithMinValue_ShouldReturnRating()
    {
        // Act
        var rating = Rating.Create(Rating.MinValue);

        // Assert
        rating.Value.Should().Be(1);
    }

    [Fact]
    public void Create_WithMaxValue_ShouldReturnRating()
    {
        // Act
        var rating = Rating.Create(Rating.MaxValue);

        // Assert
        rating.Value.Should().Be(5);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithValueBelowMin_ShouldThrowValidationException(int value)
    {
        // Act
        var act = () => Rating.Create(value);

        // Assert
        act.Should().Throw<ValidationException>()
            .WithMessage($"Rating must be between {Rating.MinValue} and {Rating.MaxValue}.");
    }

    [Theory]
    [InlineData(6)]
    [InlineData(10)]
    [InlineData(100)]
    public void Create_WithValueAboveMax_ShouldThrowValidationException(int value)
    {
        // Act
        var act = () => Rating.Create(value);

        // Assert
        act.Should().Throw<ValidationException>()
            .WithMessage($"Rating must be between {Rating.MinValue} and {Rating.MaxValue}.");
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        var rating1 = Rating.Create(4);
        var rating2 = Rating.Create(4);

        // Act & Assert
        rating1.Should().Be(rating2);
        (rating1 == rating2).Should().BeTrue();
        (rating1 != rating2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        var rating1 = Rating.Create(3);
        var rating2 = Rating.Create(5);

        // Act & Assert
        rating1.Should().NotBe(rating2);
        (rating1 == rating2).Should().BeFalse();
        (rating1 != rating2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var rating = Rating.Create(3);

        // Act & Assert
        rating.Equals(null).Should().BeFalse();
        (rating == null).Should().BeFalse();
        (null == rating).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldReturnSameHashCode()
    {
        // Arrange
        var rating1 = Rating.Create(4);
        var rating2 = Rating.Create(4);

        // Act & Assert
        rating1.GetHashCode().Should().Be(rating2.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var rating = Rating.Create(4);

        // Act & Assert
        rating.ToString().Should().Be("4/5");
    }

    [Fact]
    public void MinValue_ShouldBe1()
    {
        Rating.MinValue.Should().Be(1);
    }

    [Fact]
    public void MaxValue_ShouldBe5()
    {
        Rating.MaxValue.Should().Be(5);
    }
}
