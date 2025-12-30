using Ardalis.SmartEnum;

namespace Apex.API.Core.ValueObjects;

/// <summary>
/// Request priority enumeration
/// </summary>
public sealed class RequestPriority : SmartEnum<RequestPriority>
{
    public static readonly RequestPriority Low = new(nameof(Low), 0, "Low priority");
    public static readonly RequestPriority Medium = new(nameof(Medium), 1, "Medium priority");
    public static readonly RequestPriority High = new(nameof(High), 2, "High priority");
    public static readonly RequestPriority Urgent = new(nameof(Urgent), 3, "Urgent priority");

    public string Description { get; }

    private RequestPriority(string name, int value, string description) : base(name, value)
    {
        Description = description;
    }
}
