using System.ComponentModel.DataAnnotations;

namespace Readlog.Api.Requests.Authentication;

/// <summary>
/// User registration request
/// </summary>
/// <param name="UserName">Unique username (3-50 characters)</param>
/// <param name="Email">Valid email address</param>
/// <param name="Password">Password (min 8 characters, requires uppercase, lowercase, number, and special character)</param>
/// <param name="FirstName">Optional first name</param>
/// <param name="LastName">Optional last name</param>
public sealed record RegisterRequest(
    [Required]
    string UserName,

    [Required]
    [EmailAddress]
    string Email,

    [Required]
    string Password,

    string? FirstName = null,
    string? LastName = null);
