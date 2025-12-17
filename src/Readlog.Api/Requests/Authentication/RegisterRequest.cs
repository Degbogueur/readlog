namespace Readlog.Api.Requests.Authentication;

public sealed record RegisterRequest(
    string Email,
    string UserName,
    string Password,
    string? FirstName,
    string? LastName);
