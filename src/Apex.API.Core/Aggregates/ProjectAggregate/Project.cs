using Traxs.SharedKernel;
using Ardalis.GuardClauses;
using Apex.API.Core.ValueObjects;
using Apex.API.Core.Aggregates.ProjectAggregate.Events;

namespace Apex.API.Core.Aggregates.ProjectAggregate;

/// <summary>
/// Project aggregate root - represents an approved project with tasks and timeline
/// Created from an approved ProjectRequest
/// </summary>
public class Project : EntityBase, IAggregateRoot
{
    // Strong-typed ID
    private ProjectId _id;
    public new ProjectId Id
    {
        get => _id;
        private set => _id = value;
    }

    // Multi-tenant isolation
    private TenantId _tenantId;
    public TenantId TenantId
    {
        get => _tenantId;
        private set => _tenantId = value;
    }

    // Core Information
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    
    // Status & Priority
    public ProjectStatus Status { get; private set; } = ProjectStatus.Planning;
    public RequestPriority Priority { get; private set; } = RequestPriority.Medium;

    // Link to original ProjectRequest
    public Guid ProjectRequestId { get; private set; }

    // Budget & Timeline
    public decimal? Budget { get; private set; }
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public DateTime? ActualStartDate { get; private set; }
    public DateTime? ActualEndDate { get; private set; }

    // User Tracking
    public Guid CreatedByUserId { get; private set; }
    public Guid? ProjectManagerUserId { get; private set; }

    // Dates
    public DateTime CreatedDate { get; private set; }
    public DateTime? LastModifiedDate { get; private set; }

    // EF Core constructor
    private Project() { }

    /// <summary>
    /// Creates a new project from an approved ProjectRequest (factory method)
    /// </summary>
    public static Project CreateFromProjectRequest(
        TenantId tenantId,
        Guid projectRequestId,
        string name,
        string description,
        RequestPriority priority,
        Guid createdByUserId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        decimal? budget = null)
    {
        Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Guard.Against.NullOrWhiteSpace(description, nameof(description));

        if (name.Length < 3)
            throw new ArgumentException("Project name must be at least 3 characters", nameof(name));

        if (name.Length > 200)
            throw new ArgumentException("Project name cannot exceed 200 characters", nameof(name));

        var project = new Project
        {
            Id = ProjectId.CreateUnique(),
            TenantId = tenantId,
            ProjectRequestId = projectRequestId,
            Name = name,
            Description = description,
            Priority = priority,
            Status = ProjectStatus.Planning,
            CreatedByUserId = createdByUserId,
            CreatedDate = DateTime.UtcNow,
            StartDate = startDate,
            EndDate = endDate,
            Budget = budget
        };

        // Raise domain event
        project.RegisterDomainEvent(new ProjectCreatedEvent(
            project.Id,
            projectRequestId,
            project.Name,
            createdByUserId));

        return project;
    }

    /// <summary>
    /// Updates project details
    /// </summary>
    public void Update(string name, string description, RequestPriority priority, decimal? budget = null)
    {
        Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Guard.Against.NullOrWhiteSpace(description, nameof(description));

        Name = name;
        Description = description;
        Priority = priority;
        Budget = budget;
        LastModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates project timeline
    /// </summary>
    public void UpdateTimeline(DateTime? startDate, DateTime? endDate)
    {
        if (startDate.HasValue && endDate.HasValue && endDate.Value < startDate.Value)
            throw new ArgumentException("End date must be after start date");

        StartDate = startDate;
        EndDate = endDate;
        LastModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Assigns a project manager
    /// </summary>
    public void AssignProjectManager(Guid projectManagerUserId, Guid assignedByUserId)
    {
        if (Status.IsTerminal)
            throw new InvalidOperationException($"Cannot assign project manager to {Status.Name} project");

        var previousManagerId = ProjectManagerUserId;
        ProjectManagerUserId = projectManagerUserId;
        LastModifiedDate = DateTime.UtcNow;

        RegisterDomainEvent(new ProjectManagerAssignedEvent(
            Id,
            projectManagerUserId,
            assignedByUserId,
            previousManagerId));
    }

    /// <summary>
    /// Starts the project (Planning → Active)
    /// </summary>
    public void Start(Guid startedByUserId)
    {
        if (!Status.CanStart)
            throw new InvalidOperationException($"Cannot start project from {Status.Name} status");

        Status = ProjectStatus.Active;
        ActualStartDate = DateTime.UtcNow;
        LastModifiedDate = DateTime.UtcNow;

        RegisterDomainEvent(new ProjectStartedEvent(Id, startedByUserId));
    }

    /// <summary>
    /// Puts project on hold (Active → OnHold)
    /// </summary>
    public void PutOnHold(Guid userId, string reason)
    {
        if (!Status.CanPutOnHold)
            throw new InvalidOperationException($"Cannot put project on hold from {Status.Name} status");

        Guard.Against.NullOrWhiteSpace(reason, nameof(reason));

        Status = ProjectStatus.OnHold;
        LastModifiedDate = DateTime.UtcNow;

        RegisterDomainEvent(new ProjectPutOnHoldEvent(Id, userId, reason));
    }

    /// <summary>
    /// Resumes project from hold (OnHold → Active)
    /// </summary>
    public void Resume(Guid userId)
    {
        if (!Status.CanResume)
            throw new InvalidOperationException($"Cannot resume project from {Status.Name} status");

        Status = ProjectStatus.Active;
        LastModifiedDate = DateTime.UtcNow;

        RegisterDomainEvent(new ProjectResumedEvent(Id, userId));
    }

    /// <summary>
    /// Completes the project (Active → Completed)
    /// </summary>
    public void Complete(Guid completedByUserId)
    {
        if (!Status.CanComplete)
            throw new InvalidOperationException($"Cannot complete project from {Status.Name} status");

        Status = ProjectStatus.Completed;
        ActualEndDate = DateTime.UtcNow;
        LastModifiedDate = DateTime.UtcNow;

        RegisterDomainEvent(new ProjectCompletedEvent(Id, completedByUserId));
    }

    /// <summary>
    /// Cancels the project
    /// </summary>
    public void Cancel(Guid cancelledByUserId, string reason)
    {
        if (!Status.CanCancel)
            throw new InvalidOperationException($"Cannot cancel project from {Status.Name} status");

        Guard.Against.NullOrWhiteSpace(reason, nameof(reason));

        Status = ProjectStatus.Cancelled;
        LastModifiedDate = DateTime.UtcNow;

        RegisterDomainEvent(new ProjectCancelledEvent(Id, cancelledByUserId, reason));
    }

    /// <summary>
    /// Checks if project is overdue
    /// </summary>
    public bool IsOverdue()
    {
        return EndDate.HasValue && 
               DateTime.UtcNow > EndDate.Value && 
               !Status.IsTerminal;
    }

    /// <summary>
    /// Gets days until deadline (negative if overdue)
    /// </summary>
    public int? GetDaysUntilDeadline()
    {
        if (!EndDate.HasValue) return null;
        return (EndDate.Value - DateTime.UtcNow).Days;
    }

    /// <summary>
    /// Gets project duration in days (actual or planned)
    /// </summary>
    public int? GetDurationDays()
    {
        if (ActualStartDate.HasValue && ActualEndDate.HasValue)
            return (ActualEndDate.Value - ActualStartDate.Value).Days;

        if (StartDate.HasValue && EndDate.HasValue)
            return (EndDate.Value - StartDate.Value).Days;

        return null;
    }

    public override string ToString()
    {
        return $"Project: {Name} ({Status.Name})";
    }
}
