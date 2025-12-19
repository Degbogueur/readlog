using FluentAssertions;
using Readlog.Domain.Entities;

namespace Readlog.Domain.Tests.Entities;

public class RefreshTokenTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateRefreshToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "random-token-value";
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var refreshToken = RefreshToken.Create(userId, token, expiresAt);

        // Assert
        refreshToken.Should().NotBeNull();
        refreshToken.Id.Should().NotBeEmpty();
        refreshToken.UserId.Should().Be(userId);
        refreshToken.Token.Should().Be(token);
        refreshToken.ExpiresAt.Should().Be(expiresAt);
        refreshToken.IsRevoked.Should().BeFalse();
        refreshToken.RevokedAt.Should().BeNull();
        refreshToken.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Revoke_ShouldSetIsRevokedTrue()
    {
        // Arrange
        var refreshToken = RefreshToken.Create(Guid.NewGuid(), "token", DateTime.UtcNow.AddDays(7));

        // Act
        refreshToken.Revoke();

        // Assert
        refreshToken.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public void Revoke_ShouldSetRevokedAt()
    {
        // Arrange
        var refreshToken = RefreshToken.Create(Guid.NewGuid(), "token", DateTime.UtcNow.AddDays(7));

        // Act
        refreshToken.Revoke();

        // Assert
        refreshToken.RevokedAt.Should().NotBeNull();
        refreshToken.RevokedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Revoke_CalledMultipleTimes_ShouldUpdateRevokedAt()
    {
        // Arrange
        var refreshToken = RefreshToken.Create(Guid.NewGuid(), "token", DateTime.UtcNow.AddDays(7));
        refreshToken.Revoke();
        var firstRevokedAt = refreshToken.RevokedAt;

        // Act
        Thread.Sleep(10); // Small delay to ensure different timestamps
        refreshToken.Revoke();

        // Assert
        refreshToken.RevokedAt.Should().BeAfter(firstRevokedAt!.Value);
    }

    [Fact]
    public void IsExpired_WhenNotExpired_ShouldReturnFalse()
    {
        // Arrange
        var refreshToken = RefreshToken.Create(Guid.NewGuid(), "token", DateTime.UtcNow.AddDays(7));

        // Act & Assert
        refreshToken.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WhenExpired_ShouldReturnTrue()
    {
        // Arrange
        var refreshToken = RefreshToken.Create(Guid.NewGuid(), "token", DateTime.UtcNow.AddSeconds(-1));

        // Act & Assert
        refreshToken.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_WhenExactlyNow_ShouldReturnTrue()
    {
        // Arrange
        var refreshToken = RefreshToken.Create(Guid.NewGuid(), "token", DateTime.UtcNow);

        // Act & Assert
        refreshToken.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void IsActive_WhenNotRevokedAndNotExpired_ShouldReturnTrue()
    {
        // Arrange
        var refreshToken = RefreshToken.Create(Guid.NewGuid(), "token", DateTime.UtcNow.AddDays(7));

        // Act & Assert
        refreshToken.IsActive.Should().BeTrue();
    }

    [Fact]
    public void IsActive_WhenRevoked_ShouldReturnFalse()
    {
        // Arrange
        var refreshToken = RefreshToken.Create(Guid.NewGuid(), "token", DateTime.UtcNow.AddDays(7));
        refreshToken.Revoke();

        // Act & Assert
        refreshToken.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_WhenExpired_ShouldReturnFalse()
    {
        // Arrange
        var refreshToken = RefreshToken.Create(Guid.NewGuid(), "token", DateTime.UtcNow.AddSeconds(-1));

        // Act & Assert
        refreshToken.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_WhenRevokedAndExpired_ShouldReturnFalse()
    {
        // Arrange
        var refreshToken = RefreshToken.Create(Guid.NewGuid(), "token", DateTime.UtcNow.AddSeconds(-1));
        refreshToken.Revoke();

        // Act & Assert
        refreshToken.IsActive.Should().BeFalse();
    }
}
