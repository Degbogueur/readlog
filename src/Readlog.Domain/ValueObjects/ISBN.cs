using Readlog.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace Readlog.Domain.ValueObjects;

public sealed partial class ISBN
{
    public string Value { get; } = null!;

    private ISBN() { }

    private ISBN(string value)
    {
        Value = value;
    }

    public static ISBN Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationException("ISBN is required.");

        var cleanedValue = value.Replace("-", "").Replace(" ", "");

        if (!IsValidIsbn10(cleanedValue) && !IsValidIsbn13(cleanedValue))
            throw new ValidationException($"'{value}' is not a valid ISBN.");

        return new ISBN(cleanedValue);
    }

    public static ISBN? CreateOrDefault(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var cleanedValue = value.Replace("-", "").Replace(" ", "");

        if (!IsValidIsbn10(cleanedValue) && !IsValidIsbn13(cleanedValue))
            return null;

        return new ISBN(cleanedValue);
    }

    public override bool Equals(object? obj)
    {
        return obj is ISBN other && Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }

    public static bool operator ==(ISBN? left, ISBN? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(ISBN? left, ISBN? right)
    {
        return !(left == right);
    }

    private static bool IsValidIsbn10(string isbn)
    {
        if (isbn.Length != 10 || !Isbn10Regex().IsMatch(isbn))
            return false;

        var sum = 0;
        for (var i = 0; i < 9; i++)
            sum += (isbn[i] - '0') * (10 - i);

        var checkChar = isbn[9];
        sum += checkChar == 'X' ? 10 : checkChar - '0';

        return sum % 11 == 0;
    }

    private static bool IsValidIsbn13(string isbn)
    {
        if (isbn.Length != 13 || !Isbn13Regex().IsMatch(isbn))
            return false;

        var sum = 0;
        for (var i = 0; i < 13; i++)
            sum += (isbn[i] - '0') * (i % 2 == 0 ? 1 : 3);

        return sum % 10 == 0;
    }

    [GeneratedRegex(@"^\d{9}[\dX]$")]
    private static partial Regex Isbn10Regex();

    [GeneratedRegex(@"^\d{13}$")]
    private static partial Regex Isbn13Regex();
}
