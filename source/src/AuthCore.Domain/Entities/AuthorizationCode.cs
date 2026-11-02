using AuthCore.Domain.Common;

namespace AuthCore.Domain.Entities;

public class AuthorizationCode : Entity
{
    private AuthorizationCode() { }

    private AuthorizationCode(
        string          codeHash,
        Guid            clientId,
        Guid            userId,
        string          redirectUri,
        string          codeChallenge,
        string          scope,
        DateTimeOffset  createdAt,
        DateTimeOffset  expiresAt,
        DateTimeOffset? usedAt)
    {
        CodeHash        = codeHash;
        ClientId        = clientId;
        UserId          = userId;
        RedirectUri     = redirectUri;
        CodeChallenge   = codeChallenge;
        Scope           = scope;
        CreatedAt       = createdAt;
        ExpiresAt       = expiresAt;
        UsedAt          = usedAt;
    }

    public static AuthorizationCode Create(
        string          codeHash,
        Guid            clientId,
        Guid            userId,
        string          redirectUri,
        string          codeChallenge,
        string          scope,
        DateTimeOffset  createdAt,
        DateTimeOffset  expiresAt,
        DateTimeOffset? usedAt)
    {
        if (string.IsNullOrWhiteSpace(codeHash))                        { throw new DomainException("CodeHash must be set."); }
        if (clientId == Guid.Empty)                                     { throw new DomainException("ClientId must be set."); }
        if (userId == Guid.Empty)                                       { throw new DomainException("UserId must be set."); }
        if (string.IsNullOrWhiteSpace(redirectUri))                     { throw new DomainException("RedirectUri must be set."); }
        if (string.IsNullOrWhiteSpace(codeChallenge))                   { throw new DomainException("CodeChallenge must be set."); }
        if (string.IsNullOrWhiteSpace(scope))                           { throw new DomainException("Scope must be set."); }
        if (createdAt == DateTimeOffset.MinValue)                       { throw new DomainException("CreatedAt must be set."); }
        if (expiresAt <= createdAt)                                     { throw new DomainException("ExpiresAt must be later than CreatedAt."); }
        if (createdAt.Offset == TimeSpan.Zero)                          { throw new DomainException("CreatedAt must have a non-zero UTC offset."); }
        if (expiresAt.Offset == TimeSpan.Zero)                          { throw new DomainException("ExpiresAt must have a non-zero UTC offset."); }
        if (usedAt.HasValue && usedAt.Value.Offset == TimeSpan.Zero)    { throw new DomainException("UsedAt must have a non-zero UTC offset."); }
        if (usedAt.HasValue && usedAt.Value < createdAt)                { throw new DomainException("UsedAt cannot be earlier than CreatedAt."); }
        if (usedAt.HasValue && usedAt.Value > expiresAt)                { throw new DomainException("UsedAt cannot be later than ExpiresAt."); } 

        return new AuthorizationCode(codeHash.Trim(), clientId, userId, redirectUri, codeChallenge, scope.Trim(), createdAt, expiresAt, usedAt);
    }

    public void MarkAsUsed(DateTimeOffset now)
    {
        if (now.Offset == TimeSpan.Zero) { throw new DomainException("Now must have a non-zero UTC offset."); }
        if (UsedAt.HasValue)             { throw new DomainException("Authorization code has already been used."); }
        if (now < CreatedAt)             { throw new DomainException("UsedAt cannot be earlier than CreatedAt."); }
        if (now >= ExpiresAt)            { throw new DomainException("Authorization code has expired."); }
        UsedAt = now;
    }

    public bool IsActive(DateTimeOffset now)
    {
        if (now.Offset == TimeSpan.Zero) { throw new DomainException("Now must have a non-zero UTC offset."); }
        return UsedAt is null && now < ExpiresAt;
    }

    public string           CodeHash        { get; private set; } = string.Empty;
    public Guid             ClientId        { get; private set; }
    public Guid             UserId          { get; private set; }
    public string           RedirectUri     { get; private set; } = string.Empty;
    public string           CodeChallenge   { get; private set; } = string.Empty;
    public string           Scope           { get; private set; } = string.Empty; 
    public DateTimeOffset   CreatedAt       { get; private set; }
    public DateTimeOffset   ExpiresAt       { get; private set; }
    public DateTimeOffset?  UsedAt          { get; private set; }
}
