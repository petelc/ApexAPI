using Traxs.SharedKernel;
using Ardalis.GuardClauses;
using Apex.API.Core.ValueObjects;
using Apex.API.Core.Aggregates.RequestAggregate.Events;

namespace Apex.API.Core.Aggregates.RequestAggregate;

/// <summary>
/// Request aggregate root - represents a work request in the system
/// </summary>
public class Request : EntityBase, IAggregateRoot
{
    // Strong-typed ID
    private RequestId _id;
    public new RequestId Id
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

    // Status & Priority
    public RequestStatus Status { get; private set; } = RequestStatus.Draft;
    public RequestPriority Priority { get; private set; } = RequestPriority.Medium;

    // User Tracking
    public Guid CreatedByUserId { get; private set; }
    public Guid? AssignedToUserId { get; private set; }
    public Guid? ReviewedByUserId { get; private set; }
    public Guid? ApprovedByUserId { get; private set; }
    public Guid? CompletedByUserId { get; private set; }

    // Dates
    public DateTime CreatedDate { get; private set; }
    public DateTime? SubmittedDate { get; private set; }
    public DateTime? ReviewStartedDate { get; private set; }
    public DateTime? ApprovedDate { get; private set; }
    public DateTime? DeniedDate { get; private set; }
    public DateTime? CompletedDate { get; private set; }
    public DateTime? DueDate { get; private set; }
    public DateTime? LastModifiedDate { get; private set; }

    // Notes
    public string? ReviewNotes { get; private set; }
    public string? ApprovalNotes { get; private set; }
    public string? DenialReason { get; private set; }
    public string? CompletionNotes { get; private set; }

    // EF Core constructor
    private Request() { }

    /// <summary>
    /// Creates a new request (factory method)
    /// </summary>
    public static Request Create(
        TenantId tenantId,
        string title,
        string description,
        Guid createdByUserId,
        RequestPriority? priority = null,
        DateTime? dueDate = null)
    {
        Guard.Against.NullOrWhiteSpace(title, nameof(title));
        Guard.Against.NullOrWhiteSpace(description, nameof(description));

        if (title.Length < 3)
            throw new ArgumentException("Title must be at least 3 characters", nameof(title));

        if (title.Length > 200)
            throw new ArgumentException("Title cannot exceed 200 characters", nameof(title));

        if (description.Length < 10)
            throw new ArgumentException("Description must be at least 10 characters", nameof(description));

        var request = new Request
        {
            Id = RequestId.CreateUnique(),
            TenantId = tenantId,
            Title = title,
            Description = description,
            Status = RequestStatus.Draft,
            Priority = priority ?? RequestPriority.Medium,
            CreatedByUserId = createdByUserId,
            CreatedDate = DateTime.UtcNow,
            DueDate = dueDate
        };

        // Raise domain event
        request.RegisterDomainEvent(new RequestCreatedEvent(
            request.Id,
            request.Title,
            createdByUserId));

        return request;
    }

    /// <summary>
    /// Updates request details (only allowed in Draft status)
    /// </summary>
    public void Update(string title, string description, RequestPriority priority, DateTime? dueDate)
    {
        if (!Status.CanEdit)
            throw new InvalidOperationException($"Cannot edit request in {Status.Name} status");

        Guard.Against.NullOrWhiteSpace(title, nameof(title));
        Guard.Against.NullOrWhiteSpace(description, nameof(description));

        Title = title;
        Description = description;
        Priority = priority;
        DueDate = dueDate;
        LastModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Submits request for CMB review (Draft → Pending)
    /// </summary>
    public void Submit(Guid submittedByUserId)
    {
        if (Status != RequestStatus.Draft)
            throw new InvalidOperationException($"Cannot submit request from {Status.Name} status");

        Status = RequestStatus.Pending;
        SubmittedDate = DateTime.UtcNow;
        LastModifiedDate = DateTime.UtcNow;

        RegisterDomainEvent(new RequestSubmittedEvent(Id, submittedByUserId));
    }

    /// <summary>
    /// Starts CMB review (Pending → InReview)
    /// </summary>
    public void StartReview(Guid reviewedByUserId, string? notes = null)
    {
        if (!Status.CanReview)
            throw new InvalidOperationException($"Cannot start review from {Status.Name} status");

        Status = RequestStatus.InReview;
        ReviewedByUserId = reviewedByUserId;
        ReviewStartedDate = DateTime.UtcNow;
        ReviewNotes = notes;
        LastModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Approves the request (InReview/Pending → Approved)
    /// </summary>
    public void Approve(Guid approvedByUserId, string? notes = null)
    {
        if (!Status.CanReview)
            throw new InvalidOperationException($"Cannot approve request from {Status.Name} status");

        Status = RequestStatus.Approved;
        ApprovedByUserId = approvedByUserId;
        ApprovedDate = DateTime.UtcNow;
        ApprovalNotes = notes;
        LastModifiedDate = DateTime.UtcNow;

        RegisterDomainEvent(new RequestApprovedEvent(Id, approvedByUserId, notes));
    }

    /// <summary>
    /// Denies the request (InReview/Pending → Denied)
    /// </summary>
    public void Deny(Guid deniedByUserId, string reason)
    {
        if (!Status.CanReview)
            throw new InvalidOperationException($"Cannot deny request from {Status.Name} status");

        Guard.Against.NullOrWhiteSpace(reason, nameof(reason));

        Status = RequestStatus.Denied;
        DeniedDate = DateTime.UtcNow;
        DenialReason = reason;
        LastModifiedDate = DateTime.UtcNow;

        RegisterDomainEvent(new RequestRejectedEvent(Id, deniedByUserId, reason));
    }

    /// <summary>
    /// Assigns the request to a user
    /// </summary>
    public void AssignTo(Guid userId, Guid assignedByUserId)
    {
        if (!Status.CanCreateTasks && Status != RequestStatus.InProgress)
            throw new InvalidOperationException($"Cannot assign request in {Status.Name} status");

        AssignedToUserId = userId;

        // Auto-transition to InProgress if approved
        if (Status == RequestStatus.Approved)
        {
            Status = RequestStatus.InProgress;
        }

        LastModifiedDate = DateTime.UtcNow;

        RegisterDomainEvent(new RequestAssignedEvent(Id, userId, assignedByUserId));
    }

    /// <summary>
    /// Marks request as completed (InProgress → Completed)
    /// </summary>
    public void Complete(Guid completedByUserId, string? notes = null)
    {
        if (Status != RequestStatus.InProgress)
            throw new InvalidOperationException($"Cannot complete request from {Status.Name} status");

        Status = RequestStatus.Completed;
        CompletedByUserId = completedByUserId;
        CompletedDate = DateTime.UtcNow;
        CompletionNotes = notes;
        LastModifiedDate = DateTime.UtcNow;

        RegisterDomainEvent(new RequestCompletedEvent(Id, completedByUserId));
    }

    /// <summary>
    /// Cancels the request
    /// </summary>
    public void Cancel()
    {
        if (Status.IsTerminal)
            throw new InvalidOperationException($"Cannot cancel request in {Status.Name} status");

        Status = RequestStatus.Cancelled;
        LastModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if request is overdue
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
        return $"Request: {Title} ({Status.Name})";
    }
}