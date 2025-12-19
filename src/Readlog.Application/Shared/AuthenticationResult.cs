namespace Readlog.Application.Shared;

public sealed record AuthenticationResult(
    bool Succeeded,
    string? AccessToken = null,
    string? RefreshToken = null,
    DateTime? AccessTokenExpiresAt = null,
    IEnumerable<string>? Errors = null,
    AuthErrorType ErrorType = AuthErrorType.None)
{
    public static AuthenticationResult Success(string accessToken, string refreshToken, DateTime accessTokenExpiresAt)
        => new(Succeeded: true, accessToken, refreshToken, accessTokenExpiresAt, null, AuthErrorType.None);

    public static AuthenticationResult Failure(string[] errors, AuthErrorType errorType)
        => new(Succeeded: false, Errors: errors, ErrorType: errorType);

    public static AuthenticationResult Failure(string error, AuthErrorType errorType)
        => Failure([error], errorType);
}

public enum AuthErrorType
{
    None,
    Validation,
    Conflict,
    Unauthorized
}
