namespace Readlog.Application.Shared;

public sealed record AuthenticationResult(
    bool Succeeded,
    string? AccessToken = null,
    string? RefreshToken = null,
    DateTime? AccessTokenExpiresAt = null,
    IEnumerable<string>? Errors = null)
{
    public static AuthenticationResult Success(string accessToken, string refreshToken, DateTime accessTokenExpiresAt)
        => new(Succeeded: true, accessToken, refreshToken, accessTokenExpiresAt);

    public static AuthenticationResult Failure(params string[] errors)
        => new(Succeeded: false, Errors: errors);
}
