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
 
    public static User Create(string email, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(email))               { throw new DomainException("Email can't be empty."); }
        email = email.Trim();
        email = email.ToLowerInvariant();
        if (!new EmailAddressAttribute().IsValid(email))    { throw new DomainException("Incorrect email format."); }

        DateTimeOffsetValidator.ValidateIsSet(now, nameof(now));
        DateTimeOffsetValidator.ValidateIsUtc(now, nameof(now)); 
        
        return new User(email, UserStatus.Pending, now, now);
    }
 
    public void Activate(DateTimeOffset now)
    {
        if (Status is UserStatus.Active or UserStatus.Expired) { throw new DomainException("User cannot be activated."); }
        ChangeStatus(UserStatus.Active, now);
    }

    public void Deactivate(DateTimeOffset now)
    { 
        if (Status is UserStatus.Inactive or UserStatus.Expired) { throw new DomainException("User cannot be deactivated."); }
        ChangeStatus(UserStatus.Inactive, now);
    }

    public void Lock(DateTimeOffset now)
    {
        if (Status is UserStatus.Locked or UserStatus.Expired) { throw new DomainException("User cannot be locked."); }
        ChangeStatus(UserStatus.Locked, now);
    }

    public void Expire(DateTimeOffset now)
    {
        if (Status is UserStatus.Expired) { throw new DomainException("User is already expired."); }
        ChangeStatus(UserStatus.Expired, now);
    }

    private void ChangeStatus(UserStatus status, DateTimeOffset now)
    {
        DateTimeOffsetValidator.ValidateIsSet(now, nameof(now));
        DateTimeOffsetValidator.ValidateIsUtc(now, nameof(now));
        DateTimeOffsetValidator.ValidateNotEarlier(now, UpdatedAt, nameof(now), nameof(UpdatedAt)); 

        Status    = status;
        UpdatedAt = now;
    }

    public  string          Email       { get; private set; } = string.Empty;
    public  UserStatus      Status      { get; private set; }
    public  DateTimeOffset  CreatedAt   { get; private set; }
    public  DateTimeOffset  UpdatedAt   { get; private set; }
}
