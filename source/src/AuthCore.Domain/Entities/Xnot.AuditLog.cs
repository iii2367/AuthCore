using AuthCore.Domain.Common;

namespace AuthCore.Domain.Entities;

public class AuditLog : Entity
{
    private AuditLog() { }

    private AuditLog(
        Guid            actorUserId,
        Guid            targetUserId,
        string          action,
        string?         details,
        string          ipAddress,
        DateTimeOffset  createdAt)
    {
        ActorUserId  = actorUserId;
        TargetUserId = targetUserId;
        Action       = action;
        Details      = details;
        IpAddress    = ipAddress;
        CreatedAt    = createdAt;
    }

    public static AuditLog Create(
        Guid            actorUserId,
        Guid            targetUserId,
        string          action,
        string?         details,
        string          ipAddress,
        DateTimeOffset  createdAt)
    {
        if (actorUserId == Guid.Empty)              { throw new DomainException("ActorUserId cannot be empty."); }
        if (targetUserId == Guid.Empty)             { throw new DomainException("TargetUserId cannot be empty."); }
        if (string.IsNullOrWhiteSpace(action))      { throw new DomainException("Action must be set."); }
        if (string.IsNullOrWhiteSpace(ipAddress))   { throw new DomainException("IpAddress must be set."); }
        if (createdAt == DateTimeOffset.MinValue)   { throw new DomainException("CreatedAt must be set."); }
        
        return new AuditLog(actorUserId, targetUserId, action.Trim(), details, ipAddress.Trim(), createdAt);
    }

    public Guid              ActorUserId  { get; private set; }
    public Guid              TargetUserId { get; private set; }
    public string            Action       { get; private set; } = string.Empty;
    public string?           Details      { get; private set; }
    public string            IpAddress    { get; private set; } = string.Empty;
    public DateTimeOffset    CreatedAt    { get; private set; }
}
