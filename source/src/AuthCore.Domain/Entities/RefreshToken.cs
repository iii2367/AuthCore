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
        DateTimeOffset  expiresAt)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))                                   { throw new DomainException("TokenHash must be set."); }
        if (sessionId == Guid.Empty)                                                { throw new DomainException("SessionId must be set."); }

        DateTimeOffsetValidator.ValidateIsSet(createdAt, nameof(createdAt));
        DateTimeOffsetValidator.ValidateIsSet(expiresAt, nameof(expiresAt));
        DateTimeOffsetValidator.ValidateIsUtc(createdAt, nameof(createdAt));
        DateTimeOffsetValidator.ValidateIsUtc(expiresAt, nameof(expiresAt));
        DateTimeOffsetValidator.ValidateLater(expiresAt, createdAt, nameof(expiresAt), nameof(createdAt));

        return new RefreshToken(tokenHash.Trim(), sessionId, createdAt, expiresAt, null, null);
    }

    public void Revoke(DateTimeOffset now)
    {
        if (RevokedAt.HasValue) { return; }
        DateTimeOffsetValidator.ValidateIsSet(now, nameof(now));
        DateTimeOffsetValidator.ValidateIsUtc(now, nameof(now));
        DateTimeOffsetValidator.ValidateNotEarlier(now, CreatedAt, nameof(now), nameof(CreatedAt));  

        RevokedAt = now;    
    }


    public void Replace(DateTimeOffset now, Guid newTokenId)
    {
        if (newTokenId == Guid.Empty) { throw new DomainException("New token ID must be set."); }
        if (Id == newTokenId) { throw new DomainException("Token cannot replace itself."); }
        ValidateCanChange(now);

        RevokedAt = now;
        ReplacedByTokenId = newTokenId;
    }

    public bool IsActive(DateTimeOffset now)
    {
        DateTimeOffsetValidator.ValidateIsSet(now, nameof(now));
        DateTimeOffsetValidator.ValidateIsUtc(now, nameof(now));
        return RevokedAt is null && now >= CreatedAt && now < ExpiresAt;
    }

    private void ValidateCanChange(DateTimeOffset now)
    { 
        if (!IsActive(now)) { throw new DomainException("Refresh token is not active."); }
    }

    public string           TokenHash           { get; private set; } = string.Empty;
    public Guid             SessionId           { get; private set; }
    public DateTimeOffset   CreatedAt           { get; private set; }
    public DateTimeOffset   ExpiresAt           { get; private set; }
    public DateTimeOffset?  RevokedAt           { get; private set; }
    public Guid?            ReplacedByTokenId   { get; private set; }
}
