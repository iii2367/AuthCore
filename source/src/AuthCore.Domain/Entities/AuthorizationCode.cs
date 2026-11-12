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
            DateTimeOffset  expiresAt)
    {
        if (string.IsNullOrWhiteSpace(codeHash))                        { throw new DomainException("CodeHash must be set."); }
        if (clientId == Guid.Empty)                                     { throw new DomainException("ClientId must be set."); }
        if (userId == Guid.Empty)                                       { throw new DomainException("UserId must be set."); }
        if (string.IsNullOrWhiteSpace(redirectUri))                     { throw new DomainException("RedirectUri must be set."); }
        if (string.IsNullOrWhiteSpace(codeChallenge))                   { throw new DomainException("CodeChallenge must be set."); }
        if (string.IsNullOrWhiteSpace(scope))                           { throw new DomainException("Scope must be set."); }

        DateTimeOffsetValidator.ValidateIsSet(createdAt, nameof(createdAt));
        DateTimeOffsetValidator.ValidateIsSet(expiresAt, nameof(expiresAt));
        DateTimeOffsetValidator.ValidateIsUtc(createdAt, nameof(createdAt));
        DateTimeOffsetValidator.ValidateIsUtc(expiresAt, nameof(expiresAt)); 
        DateTimeOffsetValidator.ValidateLater(expiresAt, createdAt, nameof(expiresAt), nameof(createdAt));

        return new AuthorizationCode(codeHash.Trim(), clientId, userId, redirectUri, codeChallenge, scope.Trim(), createdAt, expiresAt, null);
    }

    public void MarkAsUsed(DateTimeOffset now)
    {
        if (UsedAt.HasValue)             { throw new DomainException("Authorization code has already been used."); } 
        DateTimeOffsetValidator.ValidateIsSet(now, nameof(now));
        DateTimeOffsetValidator.ValidateIsUtc(now, nameof(now));
        DateTimeOffsetValidator.ValidateNotEarlier(now, CreatedAt, nameof(now), nameof(CreatedAt));
        if (now >= ExpiresAt)            { throw new DomainException("Authorization code has expired."); }
        UsedAt = now;
    }

    public bool IsActive(DateTimeOffset now)
    {
        DateTimeOffsetValidator.ValidateIsSet(now, nameof(now));
        DateTimeOffsetValidator.ValidateIsUtc(now, nameof(now)); 
        return UsedAt is null && now >= CreatedAt && now < ExpiresAt;
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
