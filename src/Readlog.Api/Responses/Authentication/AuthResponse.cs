namespace Readlog.Api.Responses.Authentication;

/// <summary>
/// Authentication response with tokens
/// </summary>
/// <param name="AccessToken">JWT access token for API authorization</param>
/// <param name="RefreshToken">Token to obtain new access token when expired</param>
/// <param name="ExpiresAt">Access token expiration timestamp (UTC)</param>
public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt);
