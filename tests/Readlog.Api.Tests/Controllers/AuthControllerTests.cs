using FluentAssertions;
using Readlog.Api.Requests.Authentication;
using Readlog.Api.Responses.Authentication;
using Readlog.Api.Tests.Extensions;
using Readlog.Api.Tests.Fixtures;
using System.Net;
using System.Net.Http.Json;

namespace Readlog.Api.Tests.Controllers;

[Collection("Integration")]
public class AuthControllerTests(IntegrationTestFixture fixture) : IAsyncLifetime
{
    private readonly HttpClient client = fixture.Client;

    public Task InitializeAsync() => fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Register_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var request = new RegisterRequest(
            "newuser",
            "newuser@test.com",
            "Password123!");

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        authResponse.Should().NotBeNull();
        authResponse!.AccessToken.Should().NotBeNullOrEmpty();
        authResponse.RefreshToken.Should().NotBeNullOrEmpty();
        authResponse.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturnConflict()
    {
        // Arrange
        var email = "duplicate@test.com";
        var firstRequest = new RegisterRequest("user1", email, "Password123!");
        var secondRequest = new RegisterRequest("user2", email, "Password123!");

        await client.PostAsJsonAsync("/api/auth/register", firstRequest);

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", secondRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_WithDuplicateUserName_ShouldReturnConflict()
    {
        // Arrange
        var userName = "duplicateuser";
        var firstRequest = new RegisterRequest(userName, "email1@test.com", "Password123!");
        var secondRequest = new RegisterRequest(userName, "email2@test.com", "Password123!");

        await client.PostAsJsonAsync("/api/auth/register", firstRequest);

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", secondRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Theory]
    [InlineData("", "valid@test.com", "Password123!")]
    [InlineData("user", "", "Password123!")]
    [InlineData("user", "invalid-email", "Password123!")]
    [InlineData("user", "valid@test.com", "short")]
    public async Task Register_WithInvalidData_ShouldReturnBadRequest(
        string userName,
        string email,
        string password)
    {
        // Arrange
        var request = new RegisterRequest(userName, email, password);

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidEmail_ShouldReturnSuccess()
    {
        // Arrange
        var email = "logintest@test.com";
        var password = "Password123!";
        await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("loginuser", email, password));

        var loginRequest = new LoginRequest(email, password);

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        authResponse.Should().NotBeNull();
        authResponse!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithValidUserName_ShouldReturnSuccess()
    {
        // Arrange
        var userName = "logintestuser";
        var password = "Password123!";
        await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest(userName, "logintest2@test.com", password));

        var loginRequest = new LoginRequest(userName, password);

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        authResponse.Should().NotBeNull();
        authResponse!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        var email = "wrongpass@test.com";
        await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("wrongpassuser", email, "Password123!"));

        var loginRequest = new LoginRequest(email, "WrongPassword!");

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithNonExistingUser_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest("nonexistent@test.com", "Password123!");

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithValidToken_ShouldReturnNewTokens()
    {
        // Arrange
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("refreshuser", "refresh@test.com", "Password123!"));
        var auth = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();

        var refreshRequest = new RefreshTokenRequest(auth!.RefreshToken);

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var newAuth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        newAuth.Should().NotBeNull();
        newAuth!.AccessToken.Should().NotBeNullOrEmpty();
        newAuth.RefreshToken.Should().NotBeNullOrEmpty();
        newAuth.RefreshToken.Should().NotBe(auth.RefreshToken); // Token rotation
    }

    [Fact]
    public async Task Refresh_WithInvalidToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var refreshRequest = new RefreshTokenRequest("invalid-refresh-token");

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithRevokedToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var (client, auth) = await fixture.RegisterAsync();

        // Revoke the token
        await client.PostAsJsonAsync("/api/auth/revoke",
            new RevokeTokenRequest(auth!.RefreshToken));

        // Act - Try to use revoked token
        var response = await client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshTokenRequest(auth.RefreshToken));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Revoke_WithValidToken_ShouldReturnNoContent()
    {
        // Arrange
        var (client, auth) = await fixture.RegisterAsync();

        var revokeRequest = new RevokeTokenRequest(auth!.RefreshToken);

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/revoke", revokeRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
