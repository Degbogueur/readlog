namespace Readlog.Api.Responses.Authentication;

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt);
