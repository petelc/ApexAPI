using Ardalis.SmartEnum;

namespace Apex.API.Core.ValueObjects;

/// <summary>
/// Request lifecycle status
/// </summary>
public sealed class RequestStatus : SmartEnum<RequestStatus>
{
    /// <summary>
    /// Draft - being edited by requestor
    /// </summary>
    public static readonly RequestStatus Draft = new(nameof(Draft), 0);

    /// <summary>
    /// Submitted - awaiting CMB review
    /// </summary>
    public static readonly RequestStatus Pending = new(nameof(Pending), 1);

    /// <summary>
    /// Under review by CMB
    /// </summary>
    public static readonly RequestStatus InReview = new(nameof(InReview), 2);

    /// <summary>
    /// Approved by CMB - ready for task creation
    /// </summary>
    public static readonly RequestStatus Approved = new(nameof(Approved), 3);

    /// <summary>
    /// Denied by CMB - rejected
    /// </summary>
    public static readonly RequestStatus Denied = new(nameof(Denied), 4);

    /// <summary>
    /// Work in progress - tasks being executed
    /// </summary>
    public static readonly RequestStatus InProgress = new(nameof(InProgress), 5);

    /// <summary>
    /// Completed - all tasks done
    /// </summary>
    public static readonly RequestStatus Completed = new(nameof(Completed), 6);

    /// <summary>
    /// Cancelled by requestor or admin
    /// </summary>
    public static readonly RequestStatus Cancelled = new(nameof(Cancelled), 7);

    private RequestStatus(string name, int value) : base(name, value)
    {
    }

    /// <summary>
    /// Whether the request can be edited
    /// </summary>
    public bool CanEdit => this == Draft;

    /// <summary>
    /// Whether the request can be approved/denied
    /// </summary>
    public bool CanReview => this == Pending || this == InReview;

    /// <summary>
    /// Whether tasks can be created
    /// </summary>
    public bool CanCreateTasks => this == Approved;

    /// <summary>
    /// Whether the request is in a terminal state
    /// </summary>
    public bool IsTerminal => this == Completed || this == Denied || this == Cancelled;

    /// <summary>
    /// Whether the request is active (work can be done)
    /// </summary>
    public bool IsActive => this == InProgress || this == Approved;
}
