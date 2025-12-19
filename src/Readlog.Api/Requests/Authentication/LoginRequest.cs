using System.ComponentModel.DataAnnotations;

namespace Readlog.Api.Requests.Authentication;

/// <summary>
/// User login request
/// </summary>
/// <param name="EmailOrUserName">Email address or username</param>
/// <param name="Password">User password</param>
public sealed record LoginRequest(
    [Required]
    string EmailOrUserName,

    [Required]
    string Password);