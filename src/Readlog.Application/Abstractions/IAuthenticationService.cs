using Readlog.Application.Shared;

namespace Readlog.Application.Abstractions;

public interface IAuthenticationService
{
    Task<AuthenticationResult> RegisterAsync(
        string email,
        string userName,
        string password,
        string? firstName,
        string? lastName);

    Task<AuthenticationResult> LoginAsync(
        string emailOrUserName,
        string password);

    Task<AuthenticationResult> RefreshTokenAsync(string refreshToken);

    Task<bool> RevokeTokenAsync(string refreshToken);
}
