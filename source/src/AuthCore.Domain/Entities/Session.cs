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
            DateTimeOffset  expiredAt,
            DateTimeOffset? endedAt)
    {
        UserId      = userId;
        ClientId    = clientId;
        UserAgent   = userAgent;
        IpAddress   = ipAddress;
        Status      = status;
        CreatedAt   = createdAt;
        ExpiresAt   = expiredAt;
        EndedAt     = endedAt;
    }

    public static Session Create(
            Guid            userId,
            Guid?           clientId,
            string          userAgent,
            string          ipAddress,
            DateTimeOffset  createdAt,
            DateTimeOffset  expiredAt,
            DateTimeOffset? endedAt,
            SessionStatus   status = SessionStatus.Active
            )
    {
        if (userId == Guid.Empty)                                       { throw new DomainException("UserId must be set."); }
        if (clientId.HasValue && clientId.Value == Guid.Empty)          { throw new DomainException("ClientId cannot be empty."); }
        if (string.IsNullOrWhiteSpace(userAgent))                       { throw new DomainException("UserAgent can't be empty."); }
        if (string.IsNullOrWhiteSpace(ipAddress))                       { throw new DomainException("IpAddress must be set."); } 
        if (!Enum.IsDefined(status))                                    { throw new DomainException("Invalid session status."); }
        if (createdAt == DateTimeOffset.MinValue)                       { throw new DomainException("CreatedAt must be set."); }
        if (expiredAt == DateTimeOffset.MinValue)                       { throw new DomainException("ExpiredAt must be set."); }
        if (expiredAt <= createdAt)                                     { throw new DomainException("ExpiredAt can't be earlier than CreatedAt."); }
        if (createdAt.Offset == TimeSpan.Zero)                          { throw new DomainException("CreatedAt must have a non-zero UTC offset."); }
        if (expiredAt.Offset == TimeSpan.Zero)                          { throw new DomainException("ExpiredAt must have a non-zero UTC offset."); }
        if (endedAt.HasValue && endedAt.Value.Offset == TimeSpan.Zero)  { throw new DomainException("EndedAt must have a non-zero UTC offset."); }
        if (endedAt.HasValue && endedAt.Value < createdAt)              { throw new DomainException("EndedAt can't be earlier than CreatedAt."); } 

        return new Session(userId, clientId, userAgent, ipAddress, status, createdAt, expiredAt, endedAt);
    }
 
    public void Revoke(DateTimeOffset now)
    {
        if (now.Offset == TimeSpan.Zero)        { throw new DomainException("Now must have a non-zero UTC offset."); }
        if (now == DateTimeOffset.MinValue)     { throw new DomainException("Now must be set."); }
        if (Status is not SessionStatus.Active) { throw new DomainException("Session cannot be revoke."); }
        Status = SessionStatus.Revoked;
        EndedAt = now;
    }

    public void Terminate(DateTimeOffset now) 
    {
        if (now.Offset == TimeSpan.Zero)        { throw new DomainException("Now must have a non-zero UTC offset."); }
        if (now == DateTimeOffset.MinValue)     { throw new DomainException("Now must be set."); }
        if (Status is not SessionStatus.Active) { throw new DomainException("Session cannot be terminate."); }
        Status = SessionStatus.Terminated;
        EndedAt = now;
    }

    public void Expired(DateTimeOffset now) 
    {
        if (now.Offset == TimeSpan.Zero)        { throw new DomainException("Now must have a non-zero UTC offset."); }
        if (now == DateTimeOffset.MinValue)     { throw new DomainException("Now must be set."); }
        if (Status is not SessionStatus.Active) { throw new DomainException("Session cannot be expired"); }
        Status = SessionStatus.Expired;
        EndedAt = now;
    }

    public void MarkAsCompromised(DateTimeOffset now)
    {
        if (now.Offset == TimeSpan.Zero)        { throw new DomainException("Now must have a non-zero UTC offset."); }
        if (now == DateTimeOffset.MinValue)     { throw new DomainException("Now must be set."); }
        if (Status is not SessionStatus.Active) { throw new DomainException("Session cannot mark as compromised."); }
        Status = SessionStatus.Compromised;
        EndedAt = now;
    }

    public bool IsActive(DateTimeOffset now)
    {
        if (now.Offset == TimeSpan.Zero) { throw new DomainException("Now must have a non-zero UTC offset."); }
        return Status == SessionStatus.Active && EndedAt is null && now >= CreatedAt && now < ExpiresAt; 
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
