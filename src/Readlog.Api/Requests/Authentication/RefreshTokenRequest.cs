using System.ComponentModel.DataAnnotations;

namespace Readlog.Api.Requests.Authentication;

/// <summary>
/// Request to refresh access token
/// </summary>
/// <param name="RefreshToken">Valid refresh token obtained from login or previous refresh</param>
public sealed record RefreshTokenRequest(
    [Required]
    string RefreshToken);
