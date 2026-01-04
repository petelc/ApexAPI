using Ardalis.SmartEnum;

namespace Apex.API.Core.ValueObjects;

/// <summary>
/// Change request lifecycle status
/// </summary>
public sealed class ChangeRequestStatus : SmartEnum<ChangeRequestStatus>
{
    public static readonly ChangeRequestStatus Draft = new(nameof(Draft), 1);
    public static readonly ChangeRequestStatus Submitted = new(nameof(Submitted), 2);
    public static readonly ChangeRequestStatus UnderReview = new(nameof(UnderReview), 3);
    public static readonly ChangeRequestStatus Approved = new(nameof(Approved), 4);
    public static readonly ChangeRequestStatus Denied = new(nameof(Denied), 5);
    public static readonly ChangeRequestStatus Scheduled = new(nameof(Scheduled), 6);
    public static readonly ChangeRequestStatus InProgress = new(nameof(InProgress), 7);
    public static readonly ChangeRequestStatus Completed = new(nameof(Completed), 8);
    public static readonly ChangeRequestStatus Failed = new(nameof(Failed), 9);
    public static readonly ChangeRequestStatus RolledBack = new(nameof(RolledBack), 10);
    public static readonly ChangeRequestStatus Cancelled = new(nameof(Cancelled), 11);

    private ChangeRequestStatus(string name, int value) : base(name, value)
    {
    }
}
