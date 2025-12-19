using Readlog.Application.Shared;

namespace Readlog.Application.Abstractions;

public interface IAuthenticationService
{
    Task<AuthenticationResult> RegisterAsync(
        string userName,
        string email,
        string password,
        string? firstName = null,
        string? lastName = null);

    Task<AuthenticationResult> LoginAsync(
        string emailOrUserName,
        string password);

    Task<AuthenticationResult> RefreshTokenAsync(string refreshToken);

    Task<bool> RevokeTokenAsync(string refreshToken);
}
