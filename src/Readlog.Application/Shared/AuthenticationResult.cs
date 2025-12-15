namespace Readlog.Application.Shared;

public sealed record AuthenticationResult(
    bool Succeeded,
    string? Token = null,
    DateTime? ExpiresAt = null,
    IEnumerable<string>? Errors = null)
{
    public static AuthenticationResult Success(string token, DateTime expiresAt)
        => new(Succeeded: true, token, expiresAt);

    public static AuthenticationResult Failure(params string[] errors)
        => new(Succeeded: false, Errors: errors);
}
