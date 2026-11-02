using AuthCore.Domain.Common;

namespace AuthCore.Domain.Entities;

public class Credential
{
    private Credential() {}

    private Credential(
            Guid    userId, 
            string  passwordHash)
    {
        UserId          = userId;
        PasswordHash    = passwordHash;
    }

    public static Credential Create(
            Guid    userId, 
            string  passwordHash)
    {
        if (userId == Guid.Empty)                       { throw new DomainException("UserId must be set"); }
        if (string.IsNullOrWhiteSpace(passwordHash))    { throw new DomainException("PasswordHash must be set"); }
 
        return new Credential(userId, passwordHash);
    }
    
    public void ChangePassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash)) { throw new DomainException("PasswordHash must be set"); }

        PasswordHash = passwordHash;
    }

    public Guid     UserId          { get; private set; }
    public string   PasswordHash    { get; private set; } = string.Empty;
}
