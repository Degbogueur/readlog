using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Readlog.Application.Abstractions;
using Readlog.Application.Shared;
using Readlog.Application.Shared.Constants;
using Readlog.Infrastructure.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Readlog.Infrastructure.Services;

public class AuthenticationService(
    UserManager<ApplicationUser> userManager,
    IOptions<JwtSettings> jwtSettings,
    JwtSecurityTokenHandler tokenHandler) : IAuthenticationService
{
    private readonly JwtSettings jwtSettings = jwtSettings.Value;

    public async Task<AuthenticationResult> RegisterAsync(
        string email,
        string userName,
        string password,
        string? firstName,
        string? lastName)
    {
        var user = new ApplicationUser
        {
            Email = email,
            UserName = userName,
            FirstName = firstName,
            LastName = lastName
        };

        var result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
            return AuthenticationResult.Failure(
                result.Errors.Select(e => e.Description).ToArray());

        return await GenerateAuthenticationResultAsync(user);
    }

    public async Task<AuthenticationResult> LoginAsync(string login, string password)
    {
        var user = await userManager.FindByEmailAsync(login)
            ?? await userManager.FindByNameAsync(login);

        if (user is null)
            return AuthenticationResult.Failure("Invalid email or password.");

        var isValidPassword = await userManager.CheckPasswordAsync(user, password);

        if (!isValidPassword)
            return AuthenticationResult.Failure("Invalid email or password.");

        return await GenerateAuthenticationResultAsync(user);
    }

    private async Task<AuthenticationResult> GenerateAuthenticationResultAsync(ApplicationUser user)
    {
        var key = Encoding.UTF8.GetBytes(jwtSettings.Secret);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (!string.IsNullOrWhiteSpace(user.FirstName))
            claims.Add(new Claim(CustomClaimTypes.FirstName, user.FirstName));

        if (!string.IsNullOrWhiteSpace(user.LastName))
            claims.Add(new Claim(CustomClaimTypes.LastName, user.LastName));

        var roles = await userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var expiresAt = DateTime.UtcNow.AddMinutes(jwtSettings.ExpirationInMinutes);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = jwtSettings.Issuer,
            Audience = jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return AuthenticationResult.Success(
            tokenHandler.WriteToken(token),
            expiresAt);
    }
}
