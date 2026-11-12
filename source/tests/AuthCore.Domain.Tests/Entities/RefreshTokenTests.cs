using AuthCore.Domain.Common;
using AuthCore.Domain.Entities;

namespace AuthCore.Domain.Tests.Entities;

public class RefreshTokenTests
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 11, 3, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset ExpiresAt = CreatedAt.AddDays(30);

    private static RefreshToken CreateValidToken()
    {
        return RefreshToken.Create("test-token-hash", Guid.NewGuid(), CreatedAt, ExpiresAt);
    }

    [Fact]
    public void Create_ShouldCreateToken_WhenDataIsValid()
    {
        var tokenHash = "test-token-hash";
        var sessionId = Guid.NewGuid();

        var token = RefreshToken.Create(
            tokenHash,
            sessionId,
            CreatedAt,
            ExpiresAt);

        Assert.NotNull(token);
        Assert.Equal(tokenHash, token.TokenHash);
        Assert.Equal(sessionId, token.SessionId);
        Assert.Equal(CreatedAt, token.CreatedAt);
        Assert.Equal(ExpiresAt, token.ExpiresAt);
        Assert.Null(token.RevokedAt);
        Assert.Null(token.ReplacedByTokenId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_ShouldThrowDomainException_WhenTokenHashIsInvalid(string? tokenHash)
    {
        var sessionId = Guid.NewGuid();

        Assert.Throws<DomainException>(() => RefreshToken.Create(tokenHash!, sessionId, CreatedAt, ExpiresAt));
    }

    [Fact]
    public void Create_ShouldTrimTokenHash_WhenHashContainsWhitespace()
    { 
        var tokenHash = "  test-token-hash  ";

        var token = RefreshToken.Create(
            tokenHash,
            Guid.NewGuid(),
            CreatedAt,
            ExpiresAt);

        Assert.Equal("test-token-hash", token.TokenHash);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenSessionIdIsEmpty()
    {
        Assert.Throws<DomainException>(() => RefreshToken.Create("test-token-hash", Guid.Empty, CreatedAt, ExpiresAt));
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenCreatedAtIsNotSet()
    {
        Assert.Throws<DomainException>(() => RefreshToken.Create("test-token-hash", Guid.NewGuid(), DateTimeOffset.MinValue, ExpiresAt));
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenExpiresAtIsNotSet()
    {
        Assert.Throws<DomainException>(() => RefreshToken.Create("test-token-hash", Guid.NewGuid(), CreatedAt, DateTimeOffset.MinValue));
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenCreatedAtIsNotUtc()
    {
        var nonUtcDate = new DateTimeOffset(2026, 11, 3, 12, 0, 0, TimeSpan.FromHours(2));

        Assert.Throws<DomainException>(() => RefreshToken.Create("test-token-hash", Guid.NewGuid(), nonUtcDate, ExpiresAt));
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenExpiresAtIsNotUtc()
    {
        var nonUtcDate = new DateTimeOffset(2026, 12, 3, 12, 0, 0, TimeSpan.FromHours(2));

        Assert.Throws<DomainException>(() => RefreshToken.Create("test-token-hash", Guid.NewGuid(), CreatedAt, nonUtcDate));
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenExpiresAtIsEarlierThanCreatedAt()
    { 
        var expiresAt = CreatedAt.AddDays(-1);

        Assert.Throws<DomainException>(() => RefreshToken.Create("test-token-hash", Guid.NewGuid(), CreatedAt, expiresAt));
    }


    [Fact]
    public void Create_ShouldThrowDomainException_WhenExpiresAtEqualsCreatedAt()
    {
        Assert.Throws<DomainException>(() => RefreshToken.Create("test-token-hash", Guid.NewGuid(), CreatedAt, CreatedAt));
    }

    [Fact]
    public void IsActive_ShouldReturnTrue_WhenTokenIsNewlyCreated()
    {
        var token = CreateValidToken();

        var result = token.IsActive(CreatedAt);

        Assert.True(result);
    }

    [Fact]
    public void IsActive_ShouldReturnTrue_WhenCurrentTimeIsBeforeExpiration()
    {
        var token = CreateValidToken();
        var now = CreatedAt.AddDays(10);

        var result = token.IsActive(now);

        Assert.True(result);
    }

    [Fact]
    public void IsActive_ShouldReturnFalse_WhenCurrentTimeIsBeforeCreation()
    {
        var token = CreateValidToken();
        var now = CreatedAt.AddSeconds(-1);

        var result = token.IsActive(now);

        Assert.False(result);
    }

    [Fact]
    public void IsActive_ShouldReturnFalse_WhenTokenIsExpired()
    {
        var token = CreateValidToken();
        var now = ExpiresAt.AddSeconds(1);

        var result = token.IsActive(now);

        Assert.False(result);
    }

    [Fact]
    public void IsActive_ShouldReturnFalse_WhenNowEqualsExpiresAt()
    {
        var token = CreateValidToken();

        var result = token.IsActive(ExpiresAt);

        Assert.False(result);
    }

    [Fact]
    public void IsActive_ShouldReturnFalse_WhenTokenIsRevoked()
    {
        var token = CreateValidToken();
        token.Revoke(CreatedAt.AddHours(1));

        var result = token.IsActive(CreatedAt.AddHours(2));

        Assert.False(result);
    }

    [Fact]
    public void IsActive_ShouldThrowDomainException_WhenNowIsNotSet()
    {
        var token = CreateValidToken();

        Assert.Throws<DomainException>(() => token.IsActive(DateTimeOffset.MinValue));
    }

    [Fact]
    public void IsActive_ShouldThrowDomainException_WhenNowIsNotUtc()
    {
        var token = CreateValidToken();

        var nonUtcDate = new DateTimeOffset(2026, 11, 3, 12, 0, 0, TimeSpan.FromHours(2));

        Assert.Throws<DomainException>(() => token.IsActive(nonUtcDate));
    }

    [Fact]
    public void Revoke_ShouldDoNothing_WhenAlreadyRevokedAndNowIsInvalid()
    {
        var token = CreateValidToken();

        var firstRevokeAt = CreatedAt.AddHours(1);

        token.Revoke(firstRevokeAt);

        token.Revoke(DateTimeOffset.MinValue);

        Assert.Equal(firstRevokeAt, token.RevokedAt);
    }

    [Fact]
    public void Revoke_ShouldSetRevokedAt_WhenTokenIsNotRevoked()
    {
        var token = CreateValidToken();
        var now = CreatedAt.AddHours(1);

        token.Revoke(now);

        Assert.Equal(now, token.RevokedAt);
    }

    [Fact]
    public void Revoke_ShouldNotChangeRevokedAt_WhenTokenIsAlreadyRevoked()
    {
        var token = CreateValidToken();

        var firstRevokeAt = CreatedAt.AddHours(1);
        var secondRevokeAt = CreatedAt.AddHours(2);

        token.Revoke(firstRevokeAt);

        token.Revoke(secondRevokeAt);

        Assert.Equal(firstRevokeAt, token.RevokedAt);
    }

    [Fact]
    public void Revoke_ShouldAllowRevocation_WhenTokenIsExpired()
    {
        var token = CreateValidToken();
        var now = ExpiresAt.AddDays(1);

        token.Revoke(now);

        Assert.Equal(now, token.RevokedAt);
    }

    [Fact]
    public void Revoke_ShouldThrowDomainException_WhenNowIsNotSet()
    {
        var token = CreateValidToken();

        Assert.Throws<DomainException>(() => token.Revoke(DateTimeOffset.MinValue));
    }

    [Fact]
    public void Revoke_ShouldThrowDomainException_WhenNowIsNotUtc()
    {
        var token = CreateValidToken();

        var nonUtcDate = new DateTimeOffset(2026, 11, 3, 12, 0, 0, TimeSpan.FromHours(2));

        Assert.Throws<DomainException>(() => token.Revoke(nonUtcDate));
    }

    [Fact]
    public void Revoke_ShouldThrowDomainException_WhenNowIsEarlierThanCreatedAt()
    {
        var token = CreateValidToken();
        var now = CreatedAt.AddSeconds(-1);

        Assert.Throws<DomainException>(() => token.Revoke(now));
    }

    [Fact]
    public void Replace_ShouldRevokeTokenAndSetReplacementId_WhenTokenIsActive()
    {
        var token = CreateValidToken();

        var now = CreatedAt.AddHours(1);
        var newTokenId = Guid.NewGuid();

        token.Replace(now, newTokenId);

        Assert.Equal(now, token.RevokedAt);
        Assert.Equal(newTokenId, token.ReplacedByTokenId);
    }

    [Fact]
    public void Replace_ShouldThrowDomainException_WhenNewTokenIdIsEmpty()
    {
        var token = CreateValidToken();
        var now = CreatedAt.AddHours(1);

        Assert.Throws<DomainException>(() => token.Replace(now, Guid.Empty));
    }

    [Fact]
    public void Replace_ShouldThrowDomainException_WhenTokenReplacesItself()
    {
        var token = CreateValidToken();
        var now = CreatedAt.AddHours(1);

        Assert.NotEqual(Guid.Empty, token.Id);

        Assert.Throws<DomainException>(() => token.Replace(now, token.Id));
    }

    [Fact]
    public void Replace_ShouldThrowDomainException_WhenTokenIsAlreadyRevoked()
    {
        var token = CreateValidToken();

        token.Revoke(CreatedAt.AddHours(1));

        var now = CreatedAt.AddHours(2);

        Assert.Throws<DomainException>(() => token.Replace(now, Guid.NewGuid()));
    }

    [Fact]
    public void Replace_ShouldThrowDomainException_WhenTokenIsExpired()
    {
        var token = CreateValidToken();
        var now = ExpiresAt;

        Assert.Throws<DomainException>(() => token.Replace(now, Guid.NewGuid()));
    }

    [Fact]
    public void Replace_ShouldThrowDomainException_WhenNowIsBeforeCreation()
    {
        var token = CreateValidToken();
        var now = CreatedAt.AddSeconds(-1);

        Assert.Throws<DomainException>(() => token.Replace(now, Guid.NewGuid()));
    }

    [Fact]
    public void Replace_ShouldThrowDomainException_WhenNowIsNotSet()
    {
        var token = CreateValidToken();

        Assert.Throws<DomainException>(() => token.Replace(DateTimeOffset.MinValue, Guid.NewGuid()));
    }

    [Fact]
    public void Replace_ShouldThrowDomainException_WhenNowIsNotUtc()
    {
        var token = CreateValidToken();

        var nonUtcDate = new DateTimeOffset(2026, 11, 3, 12, 0, 0, TimeSpan.FromHours(2));

        Assert.Throws<DomainException>(() => token.Replace(nonUtcDate, Guid.NewGuid()));
    }
} 
