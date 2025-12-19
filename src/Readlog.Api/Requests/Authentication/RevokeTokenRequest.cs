using System.ComponentModel.DataAnnotations;

namespace Readlog.Api.Requests.Authentication;

/// <summary>
/// Request to revoke a refresh token (logout)
/// </summary>
/// <param name="RefreshToken">Refresh token to revoke</param>
public sealed record RevokeTokenRequest(
    [Required]
    string RefreshToken);
