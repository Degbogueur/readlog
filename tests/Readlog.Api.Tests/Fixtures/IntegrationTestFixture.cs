using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Readlog.Api.Requests.Authentication;
using Readlog.Api.Responses.Authentication;
using Readlog.Domain.Entities;
using Readlog.Infrastructure.Data;
using Readlog.Infrastructure.Interceptors;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Testcontainers.MsSql;

namespace Readlog.Api.Tests.Fixtures;

public class IntegrationTestFixture : IAsyncLifetime
{
    private readonly MsSqlContainer dbContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("Strong_Password_123!")
        .Build();

    public WebApplicationFactory<Program> Factory { get; private set; } = null!;
    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await dbContainer.StartAsync();

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                    services.RemoveAll<ApplicationDbContext>();

                    services.AddScoped<AuditableEntityInterceptor>();
                    services.AddScoped<SoftDeleteInterceptor>();
                    services.AddScoped<DomainEventInterceptor>();

                    services.AddDbContext<ApplicationDbContext>((sp, options) =>
                    {
                        var auditableInterceptor = sp.GetRequiredService<AuditableEntityInterceptor>();
                        var softDeleteInterceptor = sp.GetRequiredService<SoftDeleteInterceptor>();
                        var domainEventInterceptor = sp.GetRequiredService<DomainEventInterceptor>();

                        options.UseSqlServer(dbContainer.GetConnectionString())
                            .AddInterceptors(auditableInterceptor, softDeleteInterceptor, domainEventInterceptor);
                    });

                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    db.Database.Migrate();
                });

                builder.UseEnvironment("Testing");
            });

        Client = Factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await Factory.DisposeAsync();
        await dbContainer.DisposeAsync();
    }

    /// <summary>
    /// Creates a new authenticated client with a fresh user
    /// </summary>
    public async Task<HttpClient> CreateAuthenticatedClientAsync(
        string? userName = null,
        string? email = null)
    {
        var client = Factory.CreateClient();

        userName ??= $"testuser_{Guid.NewGuid():N}";
        email ??= $"{userName}@test.com";
        var password = "Test123!@#";

        // Register user
        var registerRequest = new RegisterRequest(userName, email, password);
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();

        var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();

        // Set authorization header
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authResponse!.AccessToken);

        return client;
    }

    /// <summary>
    /// Authenticates an existing user and returns the client
    /// </summary>
    public async Task<(HttpClient Client, AuthResponse Auth)> LoginAsync(
        string emailOrUserName,
        string password)
    {
        var client = Factory.CreateClient();

        var loginRequest = new LoginRequest(emailOrUserName, password);
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authResponse!.AccessToken);

        return (client, authResponse);
    }

    /// <summary>
    /// Registers a fresh user and returns the client
    /// </summary>
    public async Task<(HttpClient Client, AuthResponse Auth)> RegisterAsync(
        string? userName = null,
        string? email = null)
    {
        var client = Factory.CreateClient();

        userName ??= $"testuser_{Guid.NewGuid():N}";
        email ??= $"{userName}@test.com";
        var password = "Test123!@#";

        // Register user
        var registerRequest = new RegisterRequest(userName, email, password);
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();

        var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();

        // Set authorization header
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authResponse!.AccessToken);

        return (client, authResponse);
    }

    /// <summary>
    /// Resets the database between tests
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Delete all data (order matters due to foreign keys)
        db.Set<ReadingListItem>().RemoveRange(db.Set<ReadingListItem>());
        db.Set<ReadingList>().RemoveRange(db.Set<ReadingList>());
        db.Set<Review>().RemoveRange(db.Set<Review>());
        db.Set<Book>().RemoveRange(db.Set<Book>());
        db.Set<RefreshToken>().RemoveRange(db.Set<RefreshToken>());

        await db.SaveChangesAsync();
    }
}

[CollectionDefinition("Integration")]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
{
}
