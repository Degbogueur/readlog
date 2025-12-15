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
        string login,
        string password);
}
