namespace Readlog.Api.Responses.Authentication;

public sealed record AuthResponse(
    string Token,
    DateTime ExpiresAt);
