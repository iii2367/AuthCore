namespace AuthCore.Domain.Common;

/// <summary>Базовий клас сутностей. Id генерується в домені (UUIDv7 — зручний для індексів).</summary>
public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.CreateVersion7();
}
