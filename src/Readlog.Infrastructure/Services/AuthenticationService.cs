using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Readlog.Application.Abstractions;
using Readlog.Application.Shared;
using Readlog.Application.Shared.Constants;
using Readlog.Domain.Abstractions;
using Readlog.Domain.Entities;
using Readlog.Infrastructure.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Readlog.Infrastructure.Services;

public class AuthenticationService(
    UserManager<ApplicationUser> userManager,
    IOptions<JwtSettings> jwtSettings,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
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

    public async Task<AuthenticationResult> LoginAsync(string emailOrUserName, string password)
    {
        var user = await userManager.FindByEmailAsync(emailOrUserName)
            ?? await userManager.FindByNameAsync(emailOrUserName);

        if (user is null)
            return AuthenticationResult.Failure("Invalid email or password.");

        var isValidPassword = await userManager.CheckPasswordAsync(user, password);

        if (!isValidPassword)
            return AuthenticationResult.Failure("Invalid email or password.");

        return await GenerateAuthenticationResultAsync(user);
    }

    public async Task<AuthenticationResult> RefreshTokenAsync(string refreshToken)
    {
        var storedToken = await refreshTokenRepository.GetByTokenAsync(refreshToken);

        if (storedToken is null || !storedToken.IsActive)
            return AuthenticationResult.Failure("Invalid or expired refresh token.");

        var user = await userManager.FindByIdAsync(storedToken.UserId.ToString());

        if (user is null)
            return AuthenticationResult.Failure("User not found.");

        storedToken.Revoke();
        refreshTokenRepository.Update(storedToken);

        var result = await GenerateAuthenticationResultAsync(user);

        await unitOfWork.SaveChangesAsync();

        return result;
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        var storedToken = await refreshTokenRepository.GetByTokenAsync(refreshToken);

        if (storedToken is null || storedToken.IsRevoked)
            return false;

        storedToken.Revoke();
        refreshTokenRepository.Update(storedToken);
        await unitOfWork.SaveChangesAsync();

        return true;
    }

    private async Task<AuthenticationResult> GenerateAuthenticationResultAsync(ApplicationUser user)
    {
        var (accessToken, expiresAt) = await GenerateAccessTokenAsync(user);
        var refreshToken = await GenerateRefreshTokenAsync(user.Id);

        return AuthenticationResult.Success(
            accessToken,
            refreshToken,
            expiresAt);        
    }

    private async Task<(string Token, DateTime ExpiresAt)> GenerateAccessTokenAsync(ApplicationUser user)
    {
        var key = Encoding.UTF8.GetBytes(jwtSettings.Secret);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.UserName!),
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

        return (tokenHandler.WriteToken(token), expiresAt);
    }

    private async Task<string> GenerateRefreshTokenAsync(Guid userId)
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        var token = Convert.ToBase64String(randomNumber);

        var refreshToken = RefreshToken.Create(
            userId,
            token,
            DateTime.UtcNow.AddDays(jwtSettings.RefreshTokenExpirationInDays));

        await refreshTokenRepository.AddAsync(refreshToken);

        return token;
    }
}
