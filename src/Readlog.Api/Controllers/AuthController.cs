using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Readlog.Api.Requests.Authentication;
using Readlog.Api.Responses;
using Readlog.Api.Responses.Authentication;
using Readlog.Application.Abstractions;

namespace Readlog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
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

        return Ok(new AuthResponse(result.Token!, result.ExpiresAt!.Value));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await authenticationService.LoginAsync(request.Login, request.Password);

        if (!result.Succeeded)
            return Problem(
                title: "Login failed",
                detail: string.Join(", ", result.Errors ?? []),
                statusCode: StatusCodes.Status400BadRequest);

        return Ok(new AuthResponse(result.Token!, result.ExpiresAt!.Value));
    }
}
