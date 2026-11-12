namespace AuthCore.Domain.Enums;

public enum SessionStatus
{
    Active,     // The session is currently active.
    Expired,    // The session has expired.
    Revoked,    // The session has been explicitly revoked.
    Terminated, // The session has been ended normally.
    Compromised // The session is suspected of being compromised 
}
