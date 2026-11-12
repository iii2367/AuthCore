
using AuthCore.Domain.Entities;
using AuthCore.Domain.Enums;
using AuthCore.Domain.Common;

namespace AuthCore.Domain.Tests.Entities;

public class UserTests
{
    private static readonly DateTimeOffset now = new(2026, 11, 3, 12, 0, 0, TimeSpan.Zero);

    private static User CreateValidUser()
    {
        return User.Create("test_email@gmail.com", now);
    }

    [Theory]
    [InlineData("test_email@gmail.com")]
    [InlineData("   test_email@gmail.com   ")]
    [InlineData("Test_Email@gmail.com")]
    public void Create_WhenEmailIsValid_DoesNotThrow(string email)
    {
        var user = User.Create(email, now);

        Assert.Equal("test_email@gmail.com", user.Email);
        Assert.Equal(UserStatus.Pending, user.Status);
        Assert.Equal(now, user.CreatedAt);
        Assert.Equal(now, user.UpdatedAt);
    }

    [Theory]
    [InlineData("@test_emailgmail.com")]
    [InlineData("test_email_gmail.com")]
    [InlineData("")]
    [InlineData("    ")]
    [InlineData(null)]
    public void Create_WhenEmailIsNotValid_ThrowsDomainException(string email)
    {
        Assert.Throws<DomainException>(() => User.Create(email, now));
    }

    [Fact]
    public void Create_WhenDateTimeIsNotSet_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => User.Create("test_email@gmail.com", default));
    }

    [Fact]
    public void Create_WhenDateTimeIsNotUtc_ThrowsDomainException()
    {
        var date = new DateTimeOffset(2026, 11, 3, 12, 0, 0, TimeSpan.FromHours(2));

        Assert.Throws<DomainException>(() => User.Create("test_email@gmail.com", date));
    }

    [Fact]
    public void Activate_WhenUserStatusIsPending_DoesNotThrow()
    {
        var user = CreateValidUser();

        user.Activate(now);

        Assert.Equal(UserStatus.Active, user.Status);
        Assert.Equal(now, user.UpdatedAt);
        Assert.Equal(now, user.CreatedAt);
    }

    [Fact]
    public void Activate_WhenUserStatusIsInactive_DoesNotThrow()
    {
        var user = CreateValidUser();
        user.Deactivate(now);

        user.Activate(now);

        Assert.Equal(UserStatus.Active, user.Status);
    }

    [Fact]
    public void Activate_WhenUserStatusIsLocked_DoesNotThrow()
    {
        var user = CreateValidUser();
        user.Lock(now);

        user.Activate(now);

        Assert.Equal(UserStatus.Active, user.Status);
    }

    [Theory]
    [InlineData(UserStatus.Active)]
    [InlineData(UserStatus.Expired)]
    public void Activate_WhenUserStatusIsActiveOrExpired_ThrowsDomainException(UserStatus status)
    {
        var user = CreateUserWithStatus(status);

        Assert.Throws<DomainException>(() => user.Activate(now));
    }

    [Fact]
    public void Deactivate_WhenUserStatusIsPending_DoesNotThrow()
    {
        var user = CreateValidUser();

        user.Deactivate(now);

        Assert.Equal(UserStatus.Inactive, user.Status);
        Assert.Equal(now, user.UpdatedAt);
        Assert.Equal(now, user.CreatedAt);
    }

    [Fact]
    public void Deactivate_WhenUserStatusIsActive_DoesNotThrow()
    {
        var user = CreateValidUser();
        user.Activate(now);

        user.Deactivate(now);

        Assert.Equal(UserStatus.Inactive, user.Status);
    }

    [Fact]
    public void Deactivate_WhenUserStatusIsLocked_DoesNotThrow()
    {
        var user = CreateValidUser();
        user.Lock(now);

        user.Deactivate(now);

        Assert.Equal(UserStatus.Inactive, user.Status);
    }

    [Theory]
    [InlineData(UserStatus.Inactive)]
    [InlineData(UserStatus.Expired)]
    public void Deactivate_WhenUserStatusIsInactiveOrExpired_ThrowsDomainException(UserStatus status)
    {
        var user = CreateUserWithStatus(status);

        Assert.Throws<DomainException>(() => user.Deactivate(now));
    }

    [Fact]
    public void Lock_WhenUserStatusIsPending_DoesNotThrow()
    {
        var user = CreateValidUser();

        user.Lock(now);

        Assert.Equal(UserStatus.Locked, user.Status);
        Assert.Equal(now, user.UpdatedAt);
        Assert.Equal(now, user.CreatedAt);
    }

    [Fact]
    public void Lock_WhenUserStatusIsActive_DoesNotThrow()
    {
        var user = CreateValidUser();
        user.Activate(now);

        user.Lock(now);

        Assert.Equal(UserStatus.Locked, user.Status);
    }

    [Fact]
    public void Lock_WhenUserStatusIsInactive_DoesNotThrow()
    {
        var user = CreateValidUser();
        user.Deactivate(now);

        user.Lock(now);

        Assert.Equal(UserStatus.Locked, user.Status);
    }

    [Theory]
    [InlineData(UserStatus.Locked)]
    [InlineData(UserStatus.Expired)]
    public void Lock_WhenUserStatusIsLockedOrExpired_ThrowsDomainException(UserStatus status)
    {
        var user = CreateUserWithStatus(status);

        Assert.Throws<DomainException>(() => user.Lock(now));
    }

    [Fact]
    public void Expire_WhenUserIsNotExpired_DoesNotThrow()
    {
        var user = CreateValidUser();

        user.Expire(now);

        Assert.Equal(UserStatus.Expired, user.Status);
        Assert.Equal(now, user.UpdatedAt);
        Assert.Equal(now, user.CreatedAt);
    }

    [Theory]
    [InlineData(UserStatus.Active)]
    [InlineData(UserStatus.Inactive)]
    [InlineData(UserStatus.Locked)]
    public void Expire_WhenUserHasDifferentStatus_DoesNotThrow(UserStatus status)
    {
        var user = CreateUserWithStatus(status);

        user.Expire(now);

        Assert.Equal(UserStatus.Expired, user.Status);
    }

    [Fact]
    public void Expire_WhenUserStatusIsExpired_ThrowsDomainException()
    {
        var user = CreateValidUser();
        user.Expire(now);

        Assert.Throws<DomainException>(() => user.Expire(now));
    }

    [Theory]
    [InlineData("Activate")]
    [InlineData("Deactivate")]
    [InlineData("Lock")]
    [InlineData("Expire")]
    public void ChangeStatus_WhenDateTimeIsNotSet_ThrowsDomainException(string operation)
    {
        var user = CreateValidUser();

        Assert.Throws<DomainException>(() => ExecuteOperation(user, operation, default));
    }

    [Theory]
    [InlineData("Activate")]
    [InlineData("Deactivate")]
    [InlineData("Lock")]
    [InlineData("Expire")]
    public void ChangeStatus_WhenDateTimeIsNotUtc_ThrowsDomainException(string operation)
    {
        var user = CreateValidUser();

        var date = new DateTimeOffset(2026, 11, 3, 12, 0, 0, TimeSpan.FromHours(2));

        Assert.Throws<DomainException>(() => ExecuteOperation(user, operation, date));
    }

    [Theory]
    [InlineData("Activate")]
    [InlineData("Deactivate")]
    [InlineData("Lock")]
    [InlineData("Expire")]
    public void ChangeStatus_WhenDateTimeIsEarlierThanUpdatedAt_ThrowsDomainException(string operation)
    {
        var user = CreateValidUser();
        var earlier = now.AddMinutes(-1);

        Assert.Throws<DomainException>(() => ExecuteOperation(user, operation, earlier));
    }

    [Theory]
    [InlineData("Activate")]
    [InlineData("Deactivate")]
    [InlineData("Lock")]
    [InlineData("Expire")]
    public void ChangeStatus_WhenDateTimeEqualsUpdatedAt_DoesNotThrow(string operation)
    {
        var user = CreateValidUser();

        ExecuteOperation(user, operation, now);

        Assert.Equal(now, user.UpdatedAt);
    }

    [Theory]
    [InlineData("Activate")]
    [InlineData("Deactivate")]
    [InlineData("Lock")]
    [InlineData("Expire")]
    public void ChangeStatus_WhenDateTimeIsInvalid_DoesNotChangeState(string operation)
    {
        var user = CreateValidUser();

        var previousStatus = user.Status;
        var previousCreatedAt = user.CreatedAt;
        var previousUpdatedAt = user.UpdatedAt;

        Assert.Throws<DomainException>(() => ExecuteOperation(user, operation, now.AddMinutes(-1)));

        Assert.Equal(previousStatus, user.Status);
        Assert.Equal(previousCreatedAt, user.CreatedAt);
        Assert.Equal(previousUpdatedAt, user.UpdatedAt);
    }

    // HELPERS 
    private static User CreateUserWithStatus(UserStatus status)
    {
        var user = CreateValidUser();

        switch (status)
        {
            case UserStatus.Pending:    { break; }
            case UserStatus.Active:     { user.Activate(now); break; }
            case UserStatus.Inactive:   { user.Deactivate(now);break; }
            case UserStatus.Locked:     { user.Lock(now); break; }
            case UserStatus.Expired:    { user.Expire(now); break; }
            default:                    { throw new ArgumentOutOfRangeException(nameof(status)); }
        }

        return user;
    }

    private static void ExecuteOperation(
        User            user,
        string          operation,
        DateTimeOffset  date)
    {
        switch (operation)
        {
            case "Activate":    { user.Activate(date); break; }
            case "Deactivate":  { user.Deactivate(date); break; }
            case "Lock":        { user.Lock(date); break; }
            case "Expire":      { user.Expire(date); break; }
            default:            { throw new ArgumentOutOfRangeException(nameof(operation)); }
        }
    }
}
