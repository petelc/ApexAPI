using Ardalis.SmartEnum;

namespace Apex.API.Core.ValueObjects;

/// <summary>
/// Project lifecycle status
/// </summary>
public sealed class ProjectStatus : SmartEnum<ProjectStatus>
{
    /// <summary>
    /// Planning - Project created but not yet started
    /// </summary>
    public static readonly ProjectStatus Planning = new(nameof(Planning), 0);

    /// <summary>
    /// Active - Project is in progress
    /// </summary>
    public static readonly ProjectStatus Active = new(nameof(Active), 1);

    /// <summary>
    /// OnHold - Project temporarily paused
    /// </summary>
    public static readonly ProjectStatus OnHold = new(nameof(OnHold), 2);

    /// <summary>
    /// Completed - Project successfully finished
    /// </summary>
    public static readonly ProjectStatus Completed = new(nameof(Completed), 3);

    /// <summary>
    /// Cancelled - Project was cancelled
    /// </summary>
    public static readonly ProjectStatus Cancelled = new(nameof(Cancelled), 4);

    private ProjectStatus(string name, int value) : base(name, value)
    {
    }

    /// <summary>
    /// Whether the project can be started
    /// </summary>
    public bool CanStart => this == Planning;

    /// <summary>
    /// Whether the project can be put on hold
    /// </summary>
    public bool CanPutOnHold => this == Active;

    /// <summary>
    /// Whether the project can be resumed
    /// </summary>
    public bool CanResume => this == OnHold;

    /// <summary>
    /// Whether the project can be completed
    /// </summary>
    public bool CanComplete => this == Active;

    /// <summary>
    /// Whether the project can be cancelled
    /// </summary>
    public bool CanCancel => this == Planning || this == Active || this == OnHold;

    /// <summary>
    /// Whether the project is in a terminal state
    /// </summary>
    public bool IsTerminal => this == Completed || this == Cancelled;

    /// <summary>
    /// Whether the project is active (accepting work)
    /// </summary>
    public bool IsActive => this == Active;
}
