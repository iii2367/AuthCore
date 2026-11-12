using AuthCore.Domain.Common;

namespace AuthCore.Domain.Entities;

public class Permission : Entity
{
    private Permission() {}

    private Permission(
            string name, 
            string description)
    {
        Name        = name;
        Description = description;
    }

    public static Permission Create(
            string name, 
            string description)
    {
        if (string.IsNullOrWhiteSpace(name)) { throw new DomainException("Name must be set."); }
        name = name.Trim();

        description = description?.Trim() ?? string.Empty;

        return new Permission(name, description);
    }
    
    public string Name          { get; private set; } = string.Empty;
    public string Description   { get; private set; } = string.Empty;
}
