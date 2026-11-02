using AuthCore.Domain.Enums;
using AuthCore.Domain.Common;

using System.ComponentModel.DataAnnotations;

namespace AuthCore.Domain.Entities;

public class User : Entity
{
    private User() {}

    private User(
            string          email,
            UserStatus      status,
            DateTimeOffset  createdAt,
            DateTimeOffset  updatedAt)
    {
        Email      = email;
        Status     = status;
        CreatedAt  = createdAt;
        UpdatedAt  = updatedAt;
    }

    public static User Create(
            string          email,
            DateTimeOffset  createdAt,
            DateTimeOffset  updatedAt,
            UserStatus      status = UserStatus.Pending) 
    {
        if (string.IsNullOrWhiteSpace(email))               { throw new DomainException("Email can't be empty."); }
        email = email.Trim();
        email = email.ToLowerInvariant();
        if (!new EmailAddressAttribute().IsValid(email))    { throw new DomainException("Incorrect email format."); }
        if (!Enum.IsDefined(status))                        { throw new DomainException("Invalid user status."); }
        if (createdAt == DateTimeOffset.MinValue)           { throw new DomainException("CreatedAt must be set."); }
        if (updatedAt == DateTimeOffset.MinValue)           { throw new DomainException("UpdatedAt must be set."); }
        if (createdAt.Offset == TimeSpan.Zero)              { throw new DomainException("CreatedAt must have a non-zero UTC offset."); }
        if (updatedAt.Offset == TimeSpan.Zero)              { throw new DomainException("UpdatedAt must have a non-zero UTC offset."); }
        if (updatedAt <= createdAt)                         { throw new DomainException("UpdatedAt can't be earlier than CreatedAt."); } 
        
        return new User(email, status, createdAt, updatedAt);
    }
 
    public void Activate(DateTimeOffset now)
    {
        if (now.Offset == TimeSpan.Zero)                       { throw new DomainException("Now must have a non-zero UTC offset."); }
        if (Status is UserStatus.Active or UserStatus.Expired) { throw new DomainException("User cannot be activated."); }
        ChangeStatus(UserStatus.Active, now);
    }

    public void Deactivate(DateTimeOffset now)
    {
        if (now.Offset == TimeSpan.Zero)                         { throw new DomainException("Now must have a non-zero UTC offset."); }
        if (Status is UserStatus.Inactive or UserStatus.Expired) { throw new DomainException("User cannot be deactivated."); }
        ChangeStatus(UserStatus.Inactive, now);
    }

    public void Lock(DateTimeOffset now)
    {
        if (now.Offset == TimeSpan.Zero)                       { throw new DomainException("Now must have a non-zero UTC offset."); }
        if (Status is UserStatus.Locked or UserStatus.Expired) { throw new DomainException("User cannot be locked."); }
        ChangeStatus(UserStatus.Locked, now);
    }

    public void Expire(DateTimeOffset now)
    {
        if (now.Offset == TimeSpan.Zero)  { throw new DomainException("Now must have a non-zero UTC offset."); }
        if (Status is UserStatus.Expired) { throw new DomainException("User is already expired."); }
        ChangeStatus(UserStatus.Expired, now);
    }

    private void ChangeStatus(UserStatus status, DateTimeOffset now)
    {
        if (now.Offset == TimeSpan.Zero)    { throw new DomainException("Now must have a non-zero UTC offset."); }
        if (now == DateTimeOffset.MinValue) { throw new DomainException("UpdatedAt must be set."); }
        if (now < UpdatedAt)                { throw new DomainException("UpdatedAt can't be earlier than previous UpdatedAt."); }

        Status    = status;
        UpdatedAt = now;
    }

    public  string          Email       { get; private set; } = string.Empty;
    public  UserStatus      Status      { get; private set; }
    public  DateTimeOffset  CreatedAt   { get; private set; }
    public  DateTimeOffset  UpdatedAt   { get; private set; }
}
