using Ardalis.SmartEnum;

namespace Apex.API.Core.ValueObjects;

/// <summary>
/// Request/Task priority levels
/// </summary>
public sealed class Priority : SmartEnum<Priority>
{
    /// <summary>
    /// Critical - immediate attention required
    /// </summary>
    public static readonly Priority Critical = new(nameof(Critical), 0, days: 1);

    /// <summary>
    /// High - urgent, complete within 3 days
    /// </summary>
    public static readonly Priority High = new(nameof(High), 1, days: 3);

    /// <summary>
    /// Medium - standard priority, complete within 7 days
    /// </summary>
    public static readonly Priority Medium = new(nameof(Medium), 2, days: 7);

    /// <summary>
    /// Low - can be deferred, complete within 14 days
    /// </summary>
    public static readonly Priority Low = new(nameof(Low), 3, days: 14);

    /// <summary>
    /// Expected completion timeframe in days
    /// </summary>
    public int ExpectedCompletionDays { get; }

    private Priority(string name, int value, int days) : base(name, value)
    {
        ExpectedCompletionDays = days;
    }

    /// <summary>
    /// Gets the due date based on priority
    /// </summary>
    public DateTime CalculateDueDate(DateTime from)
    {
        return from.AddDays(ExpectedCompletionDays);
    }

    /// <summary>
    /// Whether this is a high-priority item
    /// </summary>
    public bool IsHighPriority => this == Critical || this == High;
}
