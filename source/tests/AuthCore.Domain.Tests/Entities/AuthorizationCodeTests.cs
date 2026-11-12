using AuthCore.Domain.Common;
using AuthCore.Domain.Entities;

namespace AuthCore.Domain.Tests.Entities;

public class AuthorizationCodeTests
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 11, 3, 12, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset ExpiresAt = CreatedAt.AddMinutes(10);

    private static Guid ClientId => Guid.NewGuid();

    private static Guid UserId => Guid.NewGuid();

    private static AuthorizationCode CreateValidAuthorizationCode()
    {
        return AuthorizationCode.Create(
            "test-code-hash",
            ClientId,
            UserId,
            "https://example.com/callback",
            "test-code-challenge",
            "openid profile email",
            CreatedAt,
            ExpiresAt);
    }

    [Fact]
    public void Create_WhenDataIsValid_CreatesAuthorizationCode()
    {
        var codeHash = "test-code-hash";
        var clientId = ClientId;
        var userId = UserId;
        var redirectUri = "https://example.com/callback";
        var codeChallenge = "test-code-challenge";
        var scope = "openid profile email";

        var code = AuthorizationCode.Create(
            codeHash,
            clientId,
            userId,
            redirectUri,
            codeChallenge,
            scope,
            CreatedAt,
            ExpiresAt);

        Assert.NotNull(code);
        Assert.Equal(codeHash, code.CodeHash);
        Assert.Equal(clientId, code.ClientId);
        Assert.Equal(userId, code.UserId);
        Assert.Equal(redirectUri, code.RedirectUri);
        Assert.Equal(codeChallenge, code.CodeChallenge);
        Assert.Equal(scope, code.Scope);
        Assert.Equal(CreatedAt, code.CreatedAt);
        Assert.Equal(ExpiresAt, code.ExpiresAt);
        Assert.Null(code.UsedAt);
    }

    [Fact]
    public void Create_WhenCodeHashContainsWhitespace_TrimsCodeHash()
    {
        var code = AuthorizationCode.Create(
            "  test-code-hash  ",
            ClientId,
            UserId,
            "https://example.com/callback",
            "test-code-challenge",
            "openid profile email",
            CreatedAt,
            ExpiresAt);

        Assert.Equal("test-code-hash", code.CodeHash);
    }

    [Fact]
    public void Create_WhenScopeContainsWhitespace_TrimsScope()
    {
        var code = AuthorizationCode.Create(
            "test-code-hash",
            ClientId,
            UserId,
            "https://example.com/callback",
            "test-code-challenge",
            "  openid profile email  ",
            CreatedAt,
            ExpiresAt);

        Assert.Equal("openid profile email", code.Scope);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WhenCodeHashIsInvalid_ThrowsDomainException(string? codeHash)
    {
        Assert.Throws<DomainException>(() => AuthorizationCode.Create(codeHash!, ClientId, UserId, "https://example.com/callback", "test-code-challenge", "openid profile email", CreatedAt, ExpiresAt));
    }

    [Fact]
    public void Create_WhenClientIdIsEmpty_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => AuthorizationCode.Create("test-code-hash", Guid.Empty, UserId, "https://example.com/callback", "test-code-challenge", "openid profile email", CreatedAt, ExpiresAt));
    }

    [Fact]
    public void Create_WhenUserIdIsEmpty_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => AuthorizationCode.Create("test-code-hash", ClientId, Guid.Empty, "https://example.com/callback", "test-code-challenge", "openid profile email", CreatedAt, ExpiresAt));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WhenRedirectUriIsInvalid_ThrowsDomainException(string? redirectUri)
    {
        Assert.Throws<DomainException>(() => AuthorizationCode.Create("test-code-hash", ClientId, UserId, redirectUri!, "test-code-challenge", "openid profile email", CreatedAt, ExpiresAt));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WhenCodeChallengeIsInvalid_ThrowsDomainException(string? codeChallenge)
    {
        Assert.Throws<DomainException>(() => AuthorizationCode.Create("test-code-hash", ClientId, UserId, "https://example.com/callback", codeChallenge!, "openid profile email", CreatedAt, ExpiresAt));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WhenScopeIsInvalid_ThrowsDomainException(string? scope)
    {
        Assert.Throws<DomainException>(() => AuthorizationCode.Create("test-code-hash", ClientId, UserId, "https://example.com/callback", "test-code-challenge", scope!, CreatedAt, ExpiresAt));
    }

    [Fact]
    public void Create_WhenCreatedAtIsNotSet_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => AuthorizationCode.Create("test-code-hash", ClientId, UserId, "https://example.com/callback", "test-code-challenge", "openid profile email", DateTimeOffset.MinValue, ExpiresAt));
    }

    [Fact]
    public void Create_WhenExpiresAtIsNotSet_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => AuthorizationCode.Create( "test-code-hash", ClientId, UserId, "https://example.com/callback", "test-code-challenge", "openid profile email", CreatedAt, DateTimeOffset.MinValue));
    }

    [Fact]
    public void Create_WhenCreatedAtIsNotUtc_ThrowsDomainException()
    {
        var nonUtcDate = new DateTimeOffset(2026, 11, 3, 12, 0, 0, TimeSpan.FromHours(2));

        Assert.Throws<DomainException>(() => AuthorizationCode.Create( "test-code-hash", ClientId, UserId, "https://example.com/callback", "test-code-challenge", "openid profile email", nonUtcDate, ExpiresAt));
    }

    [Fact]
    public void Create_WhenExpiresAtIsNotUtc_ThrowsDomainException()
    {
        var nonUtcDate = new DateTimeOffset(2026, 11, 3, 12, 10, 0, TimeSpan.FromHours(2));

        Assert.Throws<DomainException>(() => AuthorizationCode.Create("test-code-hash", ClientId, UserId, "https://example.com/callback", "test-code-challenge", "openid profile email", CreatedAt, nonUtcDate));
    }

    [Fact]
    public void Create_WhenExpiresAtIsEarlierThanCreatedAt_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => AuthorizationCode.Create("test-code-hash", ClientId, UserId, "https://example.com/callback", "test-code-challenge", "openid profile email", CreatedAt, CreatedAt.AddMinutes(-1)));
    }

    [Fact]
    public void Create_WhenExpiresAtEqualsCreatedAt_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => AuthorizationCode.Create( "test-code-hash", ClientId, UserId, "https://example.com/callback", "test-code-challenge", "openid profile email", CreatedAt, CreatedAt));
    }

    [Fact]
    public void MarkAsUsed_WhenAuthorizationCodeIsActive_SetsUsedAt()
    {
        var code = CreateValidAuthorizationCode();
        var now = CreatedAt.AddMinutes(1);

        code.MarkAsUsed(now);

        Assert.Equal(now, code.UsedAt);
    }

    [Fact]
    public void MarkAsUsed_WhenNowEqualsCreatedAt_SetsUsedAt()
    {
        var code = CreateValidAuthorizationCode();

        code.MarkAsUsed(CreatedAt);

        Assert.Equal(CreatedAt, code.UsedAt);
    }

    [Fact]
    public void MarkAsUsed_WhenNowIsBeforeExpiration_SetsUsedAt()
    {
        var code = CreateValidAuthorizationCode();
        var now = ExpiresAt.AddMinutes(-1);

        code.MarkAsUsed(now);

        Assert.Equal(now, code.UsedAt);
    }

    [Fact]
    public void MarkAsUsed_WhenAuthorizationCodeIsAlreadyUsed_ThrowsDomainException()
    {
        var code = CreateValidAuthorizationCode();
        code.MarkAsUsed(CreatedAt.AddMinutes(1));

        Assert.Throws<DomainException>(() => code.MarkAsUsed(CreatedAt.AddMinutes(2)));
    }

    [Fact]
    public void MarkAsUsed_WhenAuthorizationCodeIsExpired_ThrowsDomainException()
    {
        var code = CreateValidAuthorizationCode();

        Assert.Throws<DomainException>(() => code.MarkAsUsed(ExpiresAt));
    }

    [Fact]
    public void MarkAsUsed_WhenNowIsAfterExpiration_ThrowsDomainException()
    {
        var code = CreateValidAuthorizationCode();

        Assert.Throws<DomainException>(() => code.MarkAsUsed(ExpiresAt.AddSeconds(1)));
    }

    [Fact]
    public void MarkAsUsed_WhenNowIsBeforeCreatedAt_ThrowsDomainException()
    {
        var code = CreateValidAuthorizationCode();

        Assert.Throws<DomainException>(() => code.MarkAsUsed(CreatedAt.AddSeconds(-1)));
    }

    [Fact]
    public void MarkAsUsed_WhenNowIsNotSet_ThrowsDomainException()
    {
        var code = CreateValidAuthorizationCode();

        Assert.Throws<DomainException>(() => code.MarkAsUsed(DateTimeOffset.MinValue));
    }

    [Fact]
    public void MarkAsUsed_WhenNowIsNotUtc_ThrowsDomainException()
    {
        var code = CreateValidAuthorizationCode();

        var nonUtcDate = new DateTimeOffset(2026, 11, 3, 12, 0, 0, TimeSpan.FromHours(2));

        Assert.Throws<DomainException>(() => code.MarkAsUsed(nonUtcDate));
    }

    [Fact]
    public void MarkAsUsed_WhenDateTimeIsInvalid_DoesNotChangeState()
    {
        var code = CreateValidAuthorizationCode();

        var previousUsedAt = code.UsedAt;

        Assert.Throws<DomainException>(() => code.MarkAsUsed(CreatedAt.AddMinutes(-1)));

        Assert.Equal(previousUsedAt, code.UsedAt);
    }

    [Fact]
    public void MarkAsUsed_WhenAuthorizationCodeIsExpired_DoesNotChangeState()
    {
        var code = CreateValidAuthorizationCode();

        var previousUsedAt = code.UsedAt;

        Assert.Throws<DomainException>(() => code.MarkAsUsed(ExpiresAt));

        Assert.Equal(previousUsedAt, code.UsedAt);
    }

    [Fact]
    public void MarkAsUsed_WhenAuthorizationCodeIsAlreadyUsed_DoesNotChangeUsedAt()
    {
        var code = CreateValidAuthorizationCode();
        var firstUsedAt = CreatedAt.AddMinutes(1);

        code.MarkAsUsed(firstUsedAt);

        Assert.Throws<DomainException>(() => code.MarkAsUsed(CreatedAt.AddMinutes(2)));

        Assert.Equal(firstUsedAt, code.UsedAt);
    }

    [Fact]
    public void IsActive_WhenAuthorizationCodeIsNewlyCreated_ReturnsTrue()
    {
        var code = CreateValidAuthorizationCode();

        Assert.True(code.IsActive(CreatedAt));
    }

    [Fact]
    public void IsActive_WhenNowIsBeforeExpiration_ReturnsTrue()
    {
        var code = CreateValidAuthorizationCode();

        Assert.True(code.IsActive(CreatedAt.AddMinutes(5)));
    }

    [Fact]
    public void IsActive_WhenNowEqualsCreatedAt_ReturnsTrue()
    {
        var code = CreateValidAuthorizationCode();

        Assert.True(code.IsActive(CreatedAt));
    }

    [Fact]
    public void IsActive_WhenNowIsBeforeCreatedAt_ReturnsFalse()
    {
        var code = CreateValidAuthorizationCode();

        Assert.False(code.IsActive(CreatedAt.AddSeconds(-1)));
    }

    [Fact]
    public void IsActive_WhenNowEqualsExpiresAt_ReturnsFalse()
    {
        var code = CreateValidAuthorizationCode();

        Assert.False(code.IsActive(ExpiresAt));
    }

    [Fact]
    public void IsActive_WhenNowIsAfterExpiration_ReturnsFalse()
    {
        var code = CreateValidAuthorizationCode();

        Assert.False(code.IsActive(ExpiresAt.AddSeconds(1)));
    }

    [Fact]
    public void IsActive_WhenAuthorizationCodeIsUsed_ReturnsFalse()
    {
        var code = CreateValidAuthorizationCode();

        code.MarkAsUsed(CreatedAt.AddMinutes(1));

        Assert.False(code.IsActive(CreatedAt.AddMinutes(2)));
    }

    [Fact]
    public void IsActive_WhenNowIsNotSet_ThrowsDomainException()
    {
        var code = CreateValidAuthorizationCode();

        Assert.Throws<DomainException>(() => code.IsActive(DateTimeOffset.MinValue));
    }

    [Fact]
    public void IsActive_WhenNowIsNotUtc_ThrowsDomainException()
    {
        var code = CreateValidAuthorizationCode();

        var nonUtcDate = new DateTimeOffset(2026, 11, 3, 12, 0, 0, TimeSpan.FromHours(2));

        Assert.Throws<DomainException>(() => code.IsActive(nonUtcDate));
    }
}

