using AuthCore.Domain.Enums;
using AuthCore.Domain.Common;

namespace AuthCore.Domain.Entities;

public class Session : Entity
{
    private Session() {}

    private Session(
            Guid            userId,
            Guid?           clientId,
            string          userAgent,
            string          ipAddress,
            SessionStatus   status,
            DateTimeOffset  createdAt,
            DateTimeOffset  expiresAt,
            DateTimeOffset? endedAt)
    {
        UserId      = userId;
        ClientId    = clientId;
        UserAgent   = userAgent;
        IpAddress   = ipAddress;
        Status      = status;
        CreatedAt   = createdAt;
        ExpiresAt   = expiresAt;
        EndedAt     = endedAt;
    }
 
    public static Session Create(
            Guid            userId,
            Guid?           clientId,
            string          userAgent,
            string          ipAddress,
            DateTimeOffset  createdAt,
            DateTimeOffset  expiresAt)
    {
        if (userId == Guid.Empty)                                       { throw new DomainException("UserId must be set."); }
        if (clientId.HasValue && clientId.Value == Guid.Empty)          { throw new DomainException("ClientId cannot be empty."); }
        if (string.IsNullOrWhiteSpace(userAgent))                       { throw new DomainException("UserAgent can't be empty."); }
        if (string.IsNullOrWhiteSpace(ipAddress))                       { throw new DomainException("IpAddress must be set."); } 

        DateTimeOffsetValidator.ValidateIsSet(createdAt, nameof(createdAt));
        DateTimeOffsetValidator.ValidateIsSet(expiresAt, nameof(expiresAt));
        DateTimeOffsetValidator.ValidateIsUtc(createdAt, nameof(createdAt));
        DateTimeOffsetValidator.ValidateIsUtc(expiresAt, nameof(expiresAt));
        DateTimeOffsetValidator.ValidateLater(expiresAt, createdAt, nameof(expiresAt), nameof(createdAt));

        return new Session(userId, clientId, userAgent, ipAddress, SessionStatus.Active, createdAt, expiresAt, null); 
    }
 
    public void Revoke(DateTimeOffset now)
    {
        if (Status is not SessionStatus.Active) { return; }
        ChangeStatus(SessionStatus.Revoked, now);  
    }

    public void Terminate(DateTimeOffset now)
    {
        ChangeStatus(SessionStatus.Terminated, now);
    }

    public void Expire(DateTimeOffset now)
    {
        ChangeStatus(SessionStatus.Expired, now);
    }

    public void MarkAsCompromised(DateTimeOffset now)
    {
        ChangeStatus(SessionStatus.Compromised, now);
    } 
 
    public bool IsActive(DateTimeOffset now)
    {
        DateTimeOffsetValidator.ValidateIsSet(now, nameof(now));
        DateTimeOffsetValidator.ValidateIsUtc(now, nameof(now));
 
        return Status is SessionStatus.Active && EndedAt is null && now >= CreatedAt && now < ExpiresAt;
    }

    private void ChangeStatus(SessionStatus status, DateTimeOffset now)
    {
        DateTimeOffsetValidator.ValidateIsSet(now, nameof(now));
        DateTimeOffsetValidator.ValidateIsUtc(now, nameof(now));
        DateTimeOffsetValidator.ValidateNotEarlier(now, CreatedAt, nameof(now), nameof(CreatedAt)); 
        if (Status is not SessionStatus.Active) { throw new DomainException("Session cannot change status."); }

        Status = status;
        EndedAt = now;
    }

    public Guid             UserId      { get; private set; }
    public Guid?            ClientId    { get; private set; } 
    public string           UserAgent   { get; private set; } = string.Empty;
    public string           IpAddress   { get; private set; } = string.Empty;
    public SessionStatus    Status      { get; private set; }
    public DateTimeOffset   CreatedAt   { get; private set; }
    public DateTimeOffset   ExpiresAt   { get; private set; }
    public DateTimeOffset?  EndedAt     { get; private set; }
}
