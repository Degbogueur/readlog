namespace Readlog.Api.Requests.Authentication;

public sealed record LoginRequest(
    string EmailOrUserName,
    string Password);