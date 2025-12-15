using Readlog.Domain.Exceptions;

namespace Readlog.Domain.ValueObjects;

public sealed class Rating
{
    public const int MinValue = 1;
    public const int MaxValue = 5;

    public int Value { get; }

    private Rating(int value)
    {
        Value = value;
    }

    public static Rating Create(int value)
    {
        if (value < MinValue || value > MaxValue)
            throw new ValidationException($"Rating must be between {MinValue} and {MaxValue}.");

        return new Rating(value);
    }

    public override bool Equals(object? obj)
    {
        return obj is Rating other && Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return $"{Value}/{MaxValue}";
    }

    public static bool operator ==(Rating? left, Rating? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(Rating? left, Rating? right)
    {
        return !(left == right);
    }
}
