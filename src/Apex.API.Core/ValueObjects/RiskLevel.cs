using Ardalis.SmartEnum;

namespace Apex.API.Core.ValueObjects;

/// <summary>
/// Risk level assessment for changes
/// </summary>
public sealed class RiskLevel : SmartEnum<RiskLevel>
{
    public static readonly RiskLevel Low = new(nameof(Low), 1);
    public static readonly RiskLevel Medium = new(nameof(Medium), 2);
    public static readonly RiskLevel High = new(nameof(High), 3);
    public static readonly RiskLevel Critical = new(nameof(Critical), 4);

    private RiskLevel(string name, int value) : base(name, value)
    {
    }
}
