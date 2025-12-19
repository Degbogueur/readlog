using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Readlog.Api.Requests.Authentication;
using Readlog.Api.Responses.Authentication;
using Readlog.Application.Abstractions;
using Readlog.Application.Shared;

namespace Readlog.Api.Controllers;

/// <summary>
/// Handles user authentication and token management
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController(
    IAuthenticationService authenticationService) : ControllerBase
{
    /// <summary>
    /// Register a new user account
    /// </summary>
    /// <param name="request">Registration details</param>
    /// <returns>JWT access token and refresh token</returns>
    /// <response code="200">Successfully registered</response>
    /// <response code="400">Invalid registration data</response>
    /// <response code="409">Email or username already exists</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await authenticationService.RegisterAsync(
            request.UserName,
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName);

        if (!result.Succeeded)
            return ToAuthProblem(result, "Registration failed");

        return Ok(new AuthResponse(
            result.AccessToken!,
            result.RefreshToken!,
            result.AccessTokenExpiresAt!.Value));
    }

    /// <summary>
    /// Authenticate user and get access token
    /// </summary>
    /// <param name="request">Login credentials (email or username)</param>
    /// <returns>JWT access token and refresh token</returns>
    /// <response code="200">Successfully authenticated</response>
    /// <response code="401">Invalid credentials</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await authenticationService.LoginAsync(request.EmailOrUserName, request.Password);

        if (!result.Succeeded)
            return ToAuthProblem(result, "Login failed");

        return Ok(new AuthResponse(
            result.AccessToken!,
            result.RefreshToken!,
            result.AccessTokenExpiresAt!.Value));
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    /// <param name="request">Refresh token</param>
    /// <returns>New JWT access token and refresh token</returns>
    /// <response code="200">Successfully refreshed tokens</response>
    /// <response code="401">Invalid or expired refresh token</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await authenticationService.RefreshTokenAsync(request.RefreshToken);

        if (!result.Succeeded)
            return ToAuthProblem(result, "Token refresh failed");

        return Ok(new AuthResponse(
            result.AccessToken!,
            result.RefreshToken!,
            result.AccessTokenExpiresAt!.Value));
    }

    /// <summary>
    /// Revoke a refresh token (logout)
    /// </summary>
    /// <param name="request">Refresh token to revoke</param>
    /// <response code="204">Successfully revoked</response>
    /// <response code="400">Invalid or already revoked token</response>
    [HttpPost("revoke")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request)
    {
        var result = await authenticationService.RevokeTokenAsync(request.RefreshToken);

        if (!result)
            return Problem(
                title: "Token revocation failed",
                detail: "Invalid or already revoked token",
                statusCode: StatusCodes.Status400BadRequest
            );

        return NoContent();
    }

    private ObjectResult ToAuthProblem(AuthenticationResult result, string title)
    {
        var statusCode = result.ErrorType switch
        {
            AuthErrorType.Conflict => StatusCodes.Status409Conflict,
            AuthErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status400BadRequest
        };

        return Problem(
            title: title,
            detail: string.Join(", ", result.Errors ?? []),
            statusCode: statusCode);
    }
}
