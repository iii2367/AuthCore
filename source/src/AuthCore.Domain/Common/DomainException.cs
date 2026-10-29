namespace AuthCore.Domain.Common;

/// <summary>Порушення бізнес-правила домену.</summary>
public class DomainException(string message) : Exception(message);
