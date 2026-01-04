using Ardalis.SmartEnum;

namespace Apex.API.Core.ValueObjects;

/// <summary>
/// Type of change being requested
/// </summary>
public sealed class ChangeType : SmartEnum<ChangeType>
{
    /// <summary>
    /// Standard change - pre-approved, low risk, routine
    /// </summary>
    public static readonly ChangeType Standard = new(nameof(Standard), 1);
    
    /// <summary>
    /// Normal change - requires CAB approval
    /// </summary>
    public static readonly ChangeType Normal = new(nameof(Normal), 2);
    
    /// <summary>
    /// Emergency change - fast-track for critical issues
    /// </summary>
    public static readonly ChangeType Emergency = new(nameof(Emergency), 3);

    private ChangeType(string name, int value) : base(name, value)
    {
    }
}
