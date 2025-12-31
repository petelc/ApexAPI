using Ardalis.SmartEnum;

namespace Apex.API.Core.ValueObjects;

/// <summary>
/// Project request lifecycle status
/// </summary>
public sealed class ProjectRequestStatus : SmartEnum<ProjectRequestStatus>
{
    /// <summary>
    /// Draft - being edited by requestor
    /// </summary>
    public static readonly ProjectRequestStatus Draft = new(nameof(Draft), 0);

    /// <summary>
    /// Submitted - awaiting CMB review
    /// </summary>
    public static readonly ProjectRequestStatus Pending = new(nameof(Pending), 1);

    /// <summary>
    /// Under review by CMB
    /// </summary>
    public static readonly ProjectRequestStatus InReview = new(nameof(InReview), 2);

    /// <summary>
    /// Approved by CMB - ready for project creation
    /// </summary>
    public static readonly ProjectRequestStatus Approved = new(nameof(Approved), 3);

    /// <summary>
    /// Denied by CMB - rejected
    /// </summary>
    public static readonly ProjectRequestStatus Denied = new(nameof(Denied), 4);

    /// <summary>
    /// Converted to Project - no longer a request
    /// </summary>
    public static readonly ProjectRequestStatus Converted = new(nameof(Converted), 5);

    /// <summary>
    /// Cancelled by requestor or admin
    /// </summary>
    public static readonly ProjectRequestStatus Cancelled = new(nameof(Cancelled), 6);

    private ProjectRequestStatus(string name, int value) : base(name, value)
    {
    }

    /// <summary>
    /// Whether the project request can be edited
    /// </summary>
    public bool CanEdit => this == Draft;

    /// <summary>
    /// Whether the project request can be approved/denied
    /// </summary>
    public bool CanReview => this == Pending || this == InReview;

    /// <summary>
    /// Whether the project request can be converted to a project
    /// </summary>
    public bool CanConvertToProject => this == Approved;

    /// <summary>
    /// Whether the project request is in a terminal state
    /// </summary>
    public bool IsTerminal => this == Converted || this == Denied || this == Cancelled;

    /// <summary>
    /// Whether the project request is active (awaiting action)
    /// </summary>
    public bool IsActive => this == Pending || this == InReview || this == Approved;
}
