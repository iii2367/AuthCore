using AuthCore.Domain.Common;

namespace AuthCore.Domain.Entities;

public class RefreshToken : Entity
{
    private RefreshToken() { }

    private RefreshToken(
        string          tokenHash,
        Guid            sessionId,
        DateTimeOffset  createdAt,
        DateTimeOffset  expiresAt,
        DateTimeOffset? revokedAt,
        Guid?           replacedByTokenId)
    {
        TokenHash           = tokenHash;
        SessionId           = sessionId;
        CreatedAt           = createdAt;
        ExpiresAt           = expiresAt;
        RevokedAt           = revokedAt;
        ReplacedByTokenId   = replacedByTokenId;
    }

    public static RefreshToken Create(
        string          tokenHash,
        Guid            sessionId,
        DateTimeOffset  createdAt,
        DateTimeOffset  expiresAt,
        DateTimeOffset? revokedAt,
        Guid?           replacedByTokenId)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))                                   { throw new DomainException("TokenHash must be set."); }
        if (sessionId == Guid.Empty)                                                { throw new DomainException("SessionId must be set."); }
        if (createdAt == DateTimeOffset.MinValue)                                   { throw new DomainException("CreatedAt must be set."); }
        if (expiresAt <= createdAt)                                                 { throw new DomainException("ExpiresAt must be greater than CreatedAt."); }
        if (revokedAt.HasValue && revokedAt.Value < createdAt)                      { throw new DomainException("RevokedAt cannot be earlier than CreatedAt."); }
        if (createdAt.Offset == TimeSpan.Zero)                                      { throw new DomainException("CreatedAt must have a non-zero UTC offset."); }
        if (expiresAt.Offset == TimeSpan.Zero)                                      { throw new DomainException("ExpiresAt must have a non-zero UTC offset."); }
        if (revokedAt.HasValue && revokedAt.Value.Offset == TimeSpan.Zero)          { throw new DomainException("RevokedAt must have a non-zero UTC offset."); }
        if (replacedByTokenId.HasValue && replacedByTokenId.Value == Guid.Empty)    { throw new DomainException("ReplacedByTokenId cannot be empty."); }
        if (replacedByTokenId.HasValue && !revokedAt.HasValue)                      { throw new DomainException("Replaced token must be revoked."); }

        return new RefreshToken(tokenHash.Trim(), sessionId, createdAt, expiresAt, revokedAt, replacedByTokenId);
    }

    public void Revoke(DateTimeOffset now)
    {
        if (now.Offset == TimeSpan.Zero)    { throw new DomainException("Now must have a non-zero UTC offset."); }
        if (now == DateTimeOffset.MinValue) { throw new DomainException("Operation timestamp must be set."); }
        if (!IsActive(now))                 { throw new DomainException("Refresh token is not active."); }
        RevokedAt = now;
    }

    public void Replace(DateTimeOffset now, Guid newTokenId)
    {
        if (now.Offset == TimeSpan.Zero)    { throw new DomainException("Now must have a non-zero UTC offset."); }
        if (now == DateTimeOffset.MinValue) { throw new DomainException("Operation timestamp must be set."); }
        if (!IsActive(now))                 { throw new DomainException("Refresh token is not active."); }
        if (newTokenId == Guid.Empty)       { throw new DomainException("New token ID must be set."); }

        RevokedAt = now;
        ReplacedByTokenId = newTokenId;
    }

    public bool IsActive(DateTimeOffset now)
    {
        if (now.Offset == TimeSpan.Zero) { throw new DomainException("Now must have a non-zero UTC offset."); }
        return RevokedAt is null && now >= CreatedAt && now < ExpiresAt;
    }

    public string           TokenHash           { get; private set; } = string.Empty;
    public Guid             SessionId           { get; private set; }
    public DateTimeOffset   CreatedAt           { get; private set; }
    public DateTimeOffset   ExpiresAt           { get; private set; }
    public DateTimeOffset?  RevokedAt           { get; private set; }
    public Guid?            ReplacedByTokenId   { get; private set; }
}
