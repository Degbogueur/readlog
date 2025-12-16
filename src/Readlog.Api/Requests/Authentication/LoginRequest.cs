namespace Readlog.Api.Requests.Authentication;

public sealed record LoginRequest(
    string Login,
    string Password);