namespace AuthCore.Domain.Enums;

public enum UserStatus
{ 
    Pending,    // Account created, awaiting activation.
    Active,     // The user account is active.
    Inactive,   // The user account is inactive.
    Locked,     // The user account is locked.
    Expired     // The user account has expired. 
}
