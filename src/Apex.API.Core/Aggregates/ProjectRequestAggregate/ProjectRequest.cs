using Traxs.SharedKernel;
using Ardalis.GuardClauses;
using Apex.API.Core.ValueObjects;
using Apex.API.Core.Aggregates.ProjectRequestAggregate.Events;

namespace Apex.API.Core.Aggregates.ProjectRequestAggregate;

/// <summary>
/// ProjectRequest aggregate root - represents a request for a new project/feature
/// </summary>
public class ProjectRequest : EntityBase, IAggregateRoot
{
    // Strong-typed ID
    private ProjectRequestId _id;
    public new ProjectRequestId Id
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
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public string BusinessJustification { get; private set; } = string.Empty;

    // Status & Priority
    public ProjectRequestStatus Status { get; private set; } = ProjectRequestStatus.Draft;
    public RequestPriority Priority { get; private set; } = RequestPriority.Medium;

    // User Tracking
    public Guid CreatedByUserId { get; private set; }
    public Guid? AssignedToUserId { get; private set; }
    public Guid? ReviewedByUserId { get; private set; }
    public Guid? ApprovedByUserId { get; private set; }
    public Guid? ConvertedByUserId { get; private set; }

    // Dates
    public DateTime CreatedDate { get; private set; }
    public DateTime? SubmittedDate { get; private set; }
    public DateTime? ReviewStartedDate { get; private set; }
    public DateTime? ApprovedDate { get; private set; }
    public DateTime? DeniedDate { get; private set; }
    public DateTime? ConvertedDate { get; private set; }
    public DateTime? DueDate { get; private set; }
    public DateTime? LastModifiedDate { get; private set; }

    // Notes
    public string? ReviewNotes { get; private set; }
    public string? ApprovalNotes { get; private set; }
    public string? DenialReason { get; private set; }

    // Project linkage (after conversion)
    public Guid? ProjectId { get; private set; }

    public decimal? EstimatedBudget { get; private set; }
    public DateTime? ProposedStartDate { get; private set; }
    public DateTime? ProposedEndDate { get; private set; }

    // EF Core constructor
    private ProjectRequest() { }

    /// <summary>
    /// Creates a new project request (factory method)
    /// </summary>
    public static ProjectRequest Create(
        TenantId tenantId,
        string title,
        string description,
        string businessJustification,
        Guid createdByUserId,
        RequestPriority? priority = null,
        DateTime? dueDate = null,
        decimal? estimatedBudget = null,
        DateTime? proposedStartDate = null,
        DateTime? proposedEndDate = null)
    {
        Guard.Against.NullOrWhiteSpace(title, nameof(title));
        Guard.Against.NullOrWhiteSpace(description, nameof(description));
        Guard.Against.NullOrWhiteSpace(businessJustification, nameof(businessJustification));
        if (title.Length < 3)
            throw new ArgumentException("Title must be at least 3 characters", nameof(title));

        if (title.Length > 200)
            throw new ArgumentException("Title cannot exceed 200 characters", nameof(title));

        if (description.Length < 10)
            throw new ArgumentException("Description must be at least 10 characters", nameof(description));

        if (businessJustification.Length < 10)
            throw new ArgumentException("Business Justification must be at least 10 characters", nameof(businessJustification));

        var projectRequest = new ProjectRequest
        {
            Id = ProjectRequestId.CreateUnique(),
            TenantId = tenantId,
            Title = title,
            Description = description,
            BusinessJustification = businessJustification,
            Status = ProjectRequestStatus.Draft,
            Priority = priority ?? RequestPriority.Medium,
            CreatedByUserId = createdByUserId,
            CreatedDate = DateTime.UtcNow,
            DueDate = dueDate,
            EstimatedBudget = estimatedBudget,
            ProposedStartDate = proposedStartDate,
            ProposedEndDate = proposedEndDate
        };

        // Raise domain event
        projectRequest.RegisterDomainEvent(new ProjectRequestCreatedEvent(
            projectRequest.Id,
            projectRequest.Title,
            createdByUserId));

        return projectRequest;
    }

    /// <summary>
    /// Updates project request details (only allowed in Draft status)
    /// </summary>
    public void Update(string title, string description, string businessJustification, RequestPriority priority, DateTime? dueDate, decimal? estimatedBudget, DateTime? proposedStartDate, DateTime? proposedEndDate)
    {
        if (!Status.CanEdit)
            throw new InvalidOperationException($"Cannot edit project request in {Status.Name} status");

        Guard.Against.NullOrWhiteSpace(title, nameof(title));
        Guard.Against.NullOrWhiteSpace(description, nameof(description));
        Guard.Against.NullOrWhiteSpace(businessJustification, nameof(businessJustification));

        Title = title;
        Description = description;
        BusinessJustification = businessJustification;
        Priority = priority;
        DueDate = dueDate;
        EstimatedBudget = estimatedBudget;
        ProposedStartDate = proposedStartDate;
        ProposedEndDate = proposedEndDate;
        LastModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Submits project request for CMB review (Draft → Pending)
    /// </summary>
    public void Submit(Guid submittedByUserId)
    {
        if (Status != ProjectRequestStatus.Draft)
            throw new InvalidOperationException($"Cannot submit project request from {Status.Name} status");

        Status = ProjectRequestStatus.Pending;
        SubmittedDate = DateTime.UtcNow;
        LastModifiedDate = DateTime.UtcNow;

        RegisterDomainEvent(new ProjectRequestSubmittedEvent(Id, submittedByUserId));
    }

    /// <summary>
    /// Starts CMB review (Pending → InReview)
    /// </summary>
    public void StartReview(Guid reviewedByUserId, string? notes = null)
    {
        if (!Status.CanReview)
            throw new InvalidOperationException($"Cannot start review from {Status.Name} status");

        Status = ProjectRequestStatus.InReview;
        ReviewedByUserId = reviewedByUserId;
        ReviewStartedDate = DateTime.UtcNow;
        ReviewNotes = notes;
        LastModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Approves the project request (InReview/Pending → Approved)
    /// </summary>
    public void Approve(Guid approvedByUserId, string? notes = null)
    {
        if (!Status.CanReview)
            throw new InvalidOperationException($"Cannot approve project request from {Status.Name} status");

        Status = ProjectRequestStatus.Approved;
        ApprovedByUserId = approvedByUserId;
        ApprovedDate = DateTime.UtcNow;
        ApprovalNotes = notes;
        LastModifiedDate = DateTime.UtcNow;

        RegisterDomainEvent(new ProjectRequestApprovedEvent(Id, approvedByUserId, notes));
    }

    /// <summary>
    /// Denies the project request (InReview/Pending → Denied)
    /// </summary>
    public void Deny(Guid deniedByUserId, string reason)
    {
        if (!Status.CanReview)
            throw new InvalidOperationException($"Cannot deny project request from {Status.Name} status");

        Guard.Against.NullOrWhiteSpace(reason, nameof(reason));

        Status = ProjectRequestStatus.Denied;
        DeniedDate = DateTime.UtcNow;
        DenialReason = reason;
        LastModifiedDate = DateTime.UtcNow;

        RegisterDomainEvent(new ProjectRequestRejectedEvent(Id, deniedByUserId, reason));
    }

    /// <summary>
    /// Marks project request as converted to a project (Approved → Converted)
    /// </summary>
    public void MarkAsConverted(Guid projectId, Guid convertedByUserId)
    {
        if (!Status.CanConvertToProject)
            throw new InvalidOperationException($"Cannot convert project request from {Status.Name} status");

        Status = ProjectRequestStatus.Converted;
        ProjectId = projectId;
        ConvertedByUserId = convertedByUserId;
        ConvertedDate = DateTime.UtcNow;
        LastModifiedDate = DateTime.UtcNow;

        RegisterDomainEvent(new ProjectRequestConvertedEvent(Id, projectId, convertedByUserId));
    }

    /// <summary>
    /// Cancels the project request
    /// </summary>
    public void Cancel()
    {
        if (Status.IsTerminal)
            throw new InvalidOperationException($"Cannot cancel project request in {Status.Name} status");

        Status = ProjectRequestStatus.Cancelled;
        LastModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if project request is overdue
    /// </summary>
    public bool IsOverdue()
    {
        return DueDate.HasValue &&
               DateTime.UtcNow > DueDate.Value &&
               !Status.IsTerminal;
    }

    /// <summary>
    /// Gets days until due (negative if overdue)
    /// </summary>
    public int? GetDaysUntilDue()
    {
        if (!DueDate.HasValue) return null;
        return (DueDate.Value - DateTime.UtcNow).Days;
    }

    public override string ToString()
    {
        return $"ProjectRequest: {Title} ({Status.Name})";
    }
}
