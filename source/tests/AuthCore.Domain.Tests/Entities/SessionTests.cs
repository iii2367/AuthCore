using AuthCore.Domain.Common;
using AuthCore.Domain.Entities;
using AuthCore.Domain.Enums;

namespace AuthCore.Domain.Tests.Entities;

public class SessionTests
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 11, 3, 12, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset ExpiresAt = CreatedAt.AddDays(30);

    private static Guid UserId => Guid.NewGuid();

    private static Guid ClientId => Guid.NewGuid();

    private static Session CreateValidSession()
    {
        return Session.Create(
            UserId,
            ClientId,
            "Mozilla/5.0",
            "192.168.1.100",
            CreatedAt,
            ExpiresAt);
    }

    [Fact]
    public void Create_WhenDataIsValid_CreatesSession()
    {
        var userId = UserId;
        var clientId = ClientId;
        var userAgent = "Mozilla/5.0";
        var ipAddress = "192.168.1.100";

        var session = Session.Create(
            userId,
            clientId,
            userAgent,
            ipAddress,
            CreatedAt,
            ExpiresAt);

        Assert.NotNull(session);
        Assert.Equal(userId, session.UserId);
        Assert.Equal(clientId, session.ClientId);
        Assert.Equal(userAgent, session.UserAgent);
        Assert.Equal(ipAddress, session.IpAddress);
        Assert.Equal(SessionStatus.Active, session.Status);
        Assert.Equal(CreatedAt, session.CreatedAt);
        Assert.Equal(ExpiresAt, session.ExpiresAt);
        Assert.Null(session.EndedAt);
    }

    [Fact]
    public void Create_WhenClientIdIsNull_CreatesSession()
    {
        var session = Session.Create(
            UserId,
            null,
            "Mozilla/5.0",
            "192.168.1.100",
            CreatedAt,
            ExpiresAt);

        Assert.Null(session.ClientId);
        Assert.Equal(SessionStatus.Active, session.Status);
    }

    [Fact]
    public void Create_WhenUserIdIsEmpty_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => Session.Create(Guid.Empty, ClientId, "Mozilla/5.0", "192.168.1.100", CreatedAt, ExpiresAt));
    }

    [Fact]
    public void Create_WhenClientIdIsEmpty_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => Session.Create(UserId, Guid.Empty, "Mozilla/5.0", "192.168.1.100", CreatedAt, ExpiresAt));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WhenUserAgentIsInvalid_ThrowsDomainException(string? userAgent)
    {
        Assert.Throws<DomainException>(() => Session.Create(UserId, ClientId, userAgent!, "192.168.1.100", CreatedAt, ExpiresAt));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WhenIpAddressIsInvalid_ThrowsDomainException(
        string? ipAddress)
    {
        Assert.Throws<DomainException>(() => Session.Create(UserId,  ClientId, "Mozilla/5.0", ipAddress!, CreatedAt, ExpiresAt));
    }

    [Fact]
    public void Create_WhenCreatedAtIsNotSet_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => Session.Create(UserId, ClientId, "Mozilla/5.0", "192.168.1.100", DateTimeOffset.MinValue, ExpiresAt));
    }

    [Fact]
    public void Create_WhenExpiresAtIsNotSet_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => Session.Create(UserId, ClientId, "Mozilla/5.0", "192.168.1.100", CreatedAt, DateTimeOffset.MinValue));
    }

    [Fact]
    public void Create_WhenCreatedAtIsNotUtc_ThrowsDomainException()
    {
        var nonUtcDate = new DateTimeOffset(2026, 11, 3, 12, 0, 0, TimeSpan.FromHours(2));

        Assert.Throws<DomainException>(() => Session.Create(UserId, ClientId, "Mozilla/5.0", "192.168.1.100", nonUtcDate, ExpiresAt));
    }

    [Fact]
    public void Create_WhenExpiresAtIsNotUtc_ThrowsDomainException()
    {
        var nonUtcDate = new DateTimeOffset(2026, 12, 3, 12, 0, 0, TimeSpan.FromHours(2));

        Assert.Throws<DomainException>(() => Session.Create(UserId, ClientId, "Mozilla/5.0", "192.168.1.100", CreatedAt, nonUtcDate));
    }

    [Fact]
    public void Create_WhenExpiresAtIsEarlierThanCreatedAt_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => Session.Create(UserId, ClientId, "Mozilla/5.0", "192.168.1.100", CreatedAt, CreatedAt.AddDays(-1)));
    }

    [Fact]
    public void Create_WhenExpiresAtEqualsCreatedAt_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => Session.Create(UserId, ClientId, "Mozilla/5.0", "192.168.1.100", CreatedAt, CreatedAt));
    }

    [Fact]
    public void IsActive_WhenSessionIsNewlyCreated_ReturnsTrue()
    {
        var session = CreateValidSession();

        Assert.True(session.IsActive(CreatedAt));
    }

    [Fact]
    public void IsActive_WhenNowIsBeforeExpiration_ReturnsTrue()
    {
        var session = CreateValidSession();

        Assert.True(session.IsActive(CreatedAt.AddDays(10)));
    }

    [Fact]
    public void IsActive_WhenNowIsBeforeCreation_ReturnsFalse()
    {
        var session = CreateValidSession();

        Assert.False(session.IsActive(CreatedAt.AddSeconds(-1)));
    }

    [Fact]
    public void IsActive_WhenNowEqualsExpiresAt_ReturnsFalse()
    {
        var session = CreateValidSession();

        Assert.False(session.IsActive(ExpiresAt));
    }

    [Fact]
    public void IsActive_WhenSessionIsExpired_ReturnsFalse()
    {
        var session = CreateValidSession();

        Assert.False(session.IsActive(ExpiresAt.AddSeconds(1)));
    }

    [Theory]
    [InlineData(SessionStatus.Revoked)]
    [InlineData(SessionStatus.Terminated)]
    [InlineData(SessionStatus.Expired)]
    [InlineData(SessionStatus.Compromised)]
    public void IsActive_WhenSessionHasDifferentStatus_ReturnsFalse(SessionStatus status)
    {
        var session = CreateSessionWithStatus(status);

        Assert.False(session.IsActive(CreatedAt.AddHours(1)));
    }

    [Fact]
    public void IsActive_WhenNowIsNotSet_ThrowsDomainException()
    {
        var session = CreateValidSession();

        Assert.Throws<DomainException>(() => session.IsActive(DateTimeOffset.MinValue));
    }

    [Fact]
    public void IsActive_WhenNowIsNotUtc_ThrowsDomainException()
    {
        var session = CreateValidSession();

        var nonUtcDate = new DateTimeOffset(2026, 11, 3, 12, 0, 0, TimeSpan.FromHours(2));

        Assert.Throws<DomainException>(() => session.IsActive(nonUtcDate));
    }

    [Fact]
    public void Revoke_WhenSessionIsActive_ChangesStatusToRevoked()
    {
        var session = CreateValidSession();
        var now = CreatedAt.AddHours(1);

        session.Revoke(now);

        Assert.Equal(SessionStatus.Revoked, session.Status);
        Assert.Equal(now, session.EndedAt);
    }

    [Fact]
    public void Revoke_WhenSessionIsActive_SetsEndedAt()
    {
        var session = CreateValidSession();
        var now = CreatedAt.AddHours(1);

        session.Revoke(now);

        Assert.Equal(now, session.EndedAt);
    }

    [Fact]
    public void Revoke_WhenSessionIsAlreadyRevoked_DoesNothing()
    {
        var session = CreateValidSession();
        var firstRevokeAt = CreatedAt.AddHours(1);

        session.Revoke(firstRevokeAt);
        session.Revoke(CreatedAt.AddHours(2));

        Assert.Equal(SessionStatus.Revoked, session.Status);
        Assert.Equal(firstRevokeAt, session.EndedAt);
    }

    [Theory]
    [InlineData(SessionStatus.Terminated)]
    [InlineData(SessionStatus.Expired)]
    [InlineData(SessionStatus.Compromised)]
    public void Revoke_WhenSessionIsNotActive_DoesNothing(SessionStatus status)
    {
        var session = CreateSessionWithStatus(status);
        var previousEndedAt = session.EndedAt;

        session.Revoke(DateTimeOffset.MinValue);

        Assert.Equal(status, session.Status);
        Assert.Equal(previousEndedAt, session.EndedAt);
    }

    [Fact]
    public void Terminate_WhenSessionIsActive_ChangesStatusToTerminated()
    {
        var session = CreateValidSession();
        var now = CreatedAt.AddHours(1);

        session.Terminate(now);

        Assert.Equal(SessionStatus.Terminated, session.Status);
        Assert.Equal(now, session.EndedAt);
    }

    [Fact]
    public void Expire_WhenSessionIsActive_ChangesStatusToExpired()
    {
        var session = CreateValidSession();
        var now = CreatedAt.AddHours(1);

        session.Expire(now);

        Assert.Equal(SessionStatus.Expired, session.Status);
        Assert.Equal(now, session.EndedAt);
    }

    [Fact]
    public void MarkAsCompromised_WhenSessionIsActive_ChangesStatusToCompromised()
    {
        var session = CreateValidSession();
        var now = CreatedAt.AddHours(1);

        session.MarkAsCompromised(now);

        Assert.Equal(SessionStatus.Compromised, session.Status);
        Assert.Equal(now, session.EndedAt);
    }

    [Theory]
    [InlineData("Terminate")]
    [InlineData("Expire")]
    [InlineData("MarkAsCompromised")]
    public void ChangeStatus_WhenDateTimeIsNotSet_ThrowsDomainException(string operation)
    {
        var session = CreateValidSession();

        Assert.Throws<DomainException>(() => ExecuteOperation(session, operation, DateTimeOffset.MinValue));
    }

    [Theory]
    [InlineData("Terminate")]
    [InlineData("Expire")]
    [InlineData("MarkAsCompromised")]
    public void ChangeStatus_WhenDateTimeIsNotUtc_ThrowsDomainException(string operation)
    {
        var session = CreateValidSession();

        var nonUtcDate = new DateTimeOffset(2026, 11, 3, 12, 0, 0, TimeSpan.FromHours(2));

        Assert.Throws<DomainException>(() => ExecuteOperation(session, operation, nonUtcDate));
    }

    [Theory]
    [InlineData("Terminate")]
    [InlineData("Expire")]
    [InlineData("MarkAsCompromised")]
    public void ChangeStatus_WhenDateTimeIsEarlierThanCreatedAt_ThrowsDomainException(string operation)
    {
        var session = CreateValidSession();
        var earlier = CreatedAt.AddMinutes(-1);

        Assert.Throws<DomainException>(() => ExecuteOperation(session, operation, earlier));
    }

    [Theory]
    [InlineData("Terminate")]
    [InlineData("Expire")]
    [InlineData("MarkAsCompromised")]
    public void ChangeStatus_WhenDateTimeEqualsCreatedAt_DoesNotThrow(string operation)
    {
        var session = CreateValidSession();

        ExecuteOperation(session, operation, CreatedAt);

        Assert.NotNull(session.EndedAt);
        Assert.Equal(CreatedAt, session.EndedAt);
    }

    [Theory]
    [InlineData("Terminate")]
    [InlineData("Expire")]
    [InlineData("MarkAsCompromised")]
    public void ChangeStatus_WhenSessionIsNotActive_ThrowsDomainException(string operation)
    {
        var session = CreateValidSession();
        session.Revoke(CreatedAt);

        var previousStatus = session.Status;
        var previousEndedAt = session.EndedAt;

        Assert.Throws<DomainException>(() => ExecuteOperation(session, operation, CreatedAt.AddMinutes(1)));

        Assert.Equal(previousStatus, session.Status);
        Assert.Equal(previousEndedAt, session.EndedAt);
    }

    [Theory]
    [InlineData("Terminate")]
    [InlineData("Expire")]
    [InlineData("MarkAsCompromised")]
    public void ChangeStatus_WhenDateTimeIsInvalid_DoesNotChangeState(string operation)
    {
        var session = CreateValidSession();

        var previousStatus = session.Status;
        var previousEndedAt = session.EndedAt;

        Assert.Throws<DomainException>(() => ExecuteOperation(session, operation, CreatedAt.AddMinutes(-1)));

        Assert.Equal(previousStatus, session.Status);
        Assert.Equal(previousEndedAt, session.EndedAt);
    }

    [Theory]
    [InlineData("Terminate")]
    [InlineData("Expire")]
    [InlineData("MarkAsCompromised")]
    public void ChangeStatus_WhenDateTimeIsLater_UpdatesEndedAt(string operation)
    {
        var session = CreateValidSession();
        var now = CreatedAt.AddHours(5);

        ExecuteOperation(session, operation, now);

        Assert.Equal(now, session.EndedAt);
        Assert.Equal(CreatedAt, session.CreatedAt);
        Assert.Equal(ExpiresAt, session.ExpiresAt);
    }

    // HELPERS
    private static Session CreateSessionWithStatus(SessionStatus status)
    {
        var session = CreateValidSession();
        var now = CreatedAt.AddHours(1);

        switch (status)
        {
            case SessionStatus.Active:      { break; }
            case SessionStatus.Revoked:     { session.Revoke(now); break; }
            case SessionStatus.Terminated:  { session.Terminate(now); break; }
            case SessionStatus.Expired:     { session.Expire(now); break; }
            case SessionStatus.Compromised: { session.MarkAsCompromised(now); break; }
            default:                        { throw new ArgumentOutOfRangeException(nameof(status)); }
        }

        return session;
    }

    private static void ExecuteOperation(
        Session session,
        string operation,
        DateTimeOffset now)
    {
        switch (operation)
        {
            case "Terminate":           { session.Terminate(now); break; }
            case "Expire":              { session.Expire(now); break; }
            case "MarkAsCompromised":   { session.MarkAsCompromised(now); break; }
            default:                    { throw new ArgumentOutOfRangeException(nameof(operation));}
        }
    }
}
