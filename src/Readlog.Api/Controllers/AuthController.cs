using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Readlog.Api.Requests.Authentication;
using Readlog.Api.Responses.Authentication;
using Readlog.Application.Abstractions;

namespace Readlog.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    IAuthenticationService authenticationService) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await authenticationService.RegisterAsync(
            request.Email,
            request.UserName,
            request.Password,
            request.FirstName,
            request.LastName);

        if (!result.Succeeded)
            return Problem(
                title: "Registration failed",
                detail: string.Join(", ", result.Errors ?? []),
                statusCode: StatusCodes.Status400BadRequest);

        return Ok(new AuthResponse(result.AccessToken!, result.RefreshToken!, result.AccessTokenExpiresAt!.Value));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await authenticationService.LoginAsync(request.EmailOrUserName, request.Password);

        if (!result.Succeeded)
            return Problem(
                title: "Login failed",
                detail: string.Join(", ", result.Errors ?? []),
                statusCode: StatusCodes.Status400BadRequest);

        return Ok(new AuthResponse(result.AccessToken!, result.RefreshToken!, result.AccessTokenExpiresAt!.Value));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await authenticationService.RefreshTokenAsync(request.RefreshToken);

        if (!result.Succeeded)
            return Problem(
                title: "Token refresh failed",
                detail: string.Join(", ", result.Errors ?? []),
                statusCode: StatusCodes.Status400BadRequest
            );

        return Ok(new AuthResponse(result.AccessToken!, result.RefreshToken!, result.AccessTokenExpiresAt!.Value));
    }

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
}
