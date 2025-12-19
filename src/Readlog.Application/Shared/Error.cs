namespace Readlog.Application.Shared;

public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public static readonly Error NullValue = new("Error.NullValue", "A null value was provided.");

    public static Error NotFound(string entityName, Guid id) =>
        new($"{entityName}.NotFound", $"{entityName} with ID '{id}' was not found.");

    public static Error Validation(string message) =>
        new("Validation.Error", message);

    public static Error Conflict(string message) =>
        new("Conflict.Error", message);

    public static Error Unauthorized(string message = "Unauthorized access.") =>
        new("Unauthorized.Error", message);

    public static Error Forbidden(string message = "Forbidden access.") =>
        new("Forbidden.Error", message);
}
