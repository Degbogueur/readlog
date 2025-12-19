namespace Readlog.Api.Requests.Authentication;

public sealed record RegisterRequest(
    string UserName,
    string Email,
    string Password,
    string? FirstName = null,
    string? LastName = null);
