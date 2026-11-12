namespace AuthCore.Domain.Common;

public static class DateTimeOffsetValidator
{
    public static void ValidateIsSet(
        DateTimeOffset value,
        string propertyName)
    {
        if (value == DateTimeOffset.MinValue) { throw new DomainException($"{propertyName} must be set."); }
    }

    public static void ValidateHasNonZeroOffset(
        DateTimeOffset value,
        string propertyName)
    {
        if (value.Offset == TimeSpan.Zero) { throw new DomainException($"{propertyName} must have a non-zero UTC offset."); }
    }

    public static void ValidateIsUtc(DateTimeOffset value, string propertyName)
    {
        if (value.Offset != TimeSpan.Zero) { throw new DomainException($"{propertyName} must be in UTC (offset 00:00)."); }
    }

    public static void ValidateNotEarlier(
        DateTimeOffset value,
        DateTimeOffset previous,
        string propertyName,
        string previousName)
    {
        if (value < previous) { throw new DomainException($"{propertyName} can't be earlier than {previousName}."); }
    }

    public static void ValidateLater(
        DateTimeOffset value,
        DateTimeOffset previous,
        string propertyName,
        string previousName)
    {
        if (value <= previous) { throw new DomainException($"{propertyName} must be later than {previousName}."); }
    }
}

