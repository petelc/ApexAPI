using Traxs.SharedKernel;
using Apex.API.Core.ValueObjects;
using Apex.API.Core.Aggregates.ChangeRequestAggregate.Events;

namespace Apex.API.Core.Aggregates.ChangeRequestAggregate;

/// <summary>
/// ChangeRequest aggregate root - manages change requests for CAB approval
/// </summary>
public class ChangeRequest : EntityBase<ChangeRequestId>, IAggregateRoot
{
    // Identity
    public Guid TenantId { get; private set; }

    // Basic Information
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public ChangeType ChangeType { get; private set; } = ChangeType.Normal;
    public ChangeRequestStatus Status { get; private set; } = ChangeRequestStatus.Draft;
    public RequestPriority Priority { get; private set; } = RequestPriority.Medium;
    public RiskLevel RiskLevel { get; private set; } = RiskLevel.Medium;

    // Assessment
    public string ImpactAssessment { get; private set; } = string.Empty;
    public string RollbackPlan { get; private set; } = string.Empty;
    public string AffectedSystems { get; private set; } = string.Empty; // JSON array or comma-separated

    // Scheduling
    public DateTime? ScheduledStartDate { get; private set; }
    public DateTime? ScheduledEndDate { get; private set; }
    public DateTime? ActualStartDate { get; private set; }
    public DateTime? ActualEndDate { get; private set; }
    public string? ChangeWindow { get; private set; } // e.g., "Saturday 2AM-6AM"

    // Approval Tracking
    public bool RequiresCABApproval { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public Guid? ReviewedByUserId { get; private set; }
    public Guid? ApprovedByUserId { get; private set; }

    // Notes
    public string? ReviewNotes { get; private set; }
    public string? ApprovalNotes { get; private set; }
    public string? DenialReason { get; private set; }
    public string? ImplementationNotes { get; private set; }
    public string? RollbackReason { get; private set; }

    // Timestamps
    public DateTime CreatedDate { get; private set; }
    public DateTime? SubmittedDate { get; private set; }
    public DateTime? ReviewStartedDate { get; private set; }
    public DateTime? ApprovedDate { get; private set; }
    public DateTime? DeniedDate { get; private set; }
    public DateTime? ScheduledDate { get; private set; }
    public DateTime? CompletedDate { get; private set; }
    public DateTime? FailedDate { get; private set; }
    public DateTime? RolledBackDate { get; private set; }

    // Optional: Link to related project
    public Guid? ProjectId { get; private set; }

    // EF Core constructor
    private ChangeRequest() { }

    // Factory method
    public static ChangeRequest Create(
        Guid tenantId,
        string title,
        string description,
        Guid createdByUserId,
        ChangeType changeType,
        RequestPriority priority,
        RiskLevel riskLevel,
        string impactAssessment,
        string rollbackPlan,
        string affectedSystems,
        DateTime? scheduledStartDate = null,
        DateTime? scheduledEndDate = null,
        string? changeWindow = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required", nameof(title));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required", nameof(description));

        if (string.IsNullOrWhiteSpace(impactAssessment))
            throw new ArgumentException("Impact assessment is required", nameof(impactAssessment));

        if (string.IsNullOrWhiteSpace(rollbackPlan))
            throw new ArgumentException("Rollback plan is required", nameof(rollbackPlan));

        if (string.IsNullOrWhiteSpace(affectedSystems))
            throw new ArgumentException("Affected systems must be specified", nameof(affectedSystems));

        var changeRequest = new ChangeRequest
        {
            Id = ChangeRequestId.From(Guid.NewGuid()),
            TenantId = tenantId,
            Title = title,
            Description = description,
            CreatedByUserId = createdByUserId,
            ChangeType = changeType,
            Priority = priority,
            RiskLevel = riskLevel,
            ImpactAssessment = impactAssessment,
            RollbackPlan = rollbackPlan,
            AffectedSystems = affectedSystems,
            ScheduledStartDate = scheduledStartDate,
            ScheduledEndDate = scheduledEndDate,
            ChangeWindow = changeWindow,
            RequiresCABApproval = DetermineIfCABApprovalRequired(changeType, riskLevel),
            Status = ChangeRequestStatus.Draft,
            CreatedDate = DateTime.UtcNow
        };

        changeRequest.RegisterDomainEvent(new ChangeRequestCreatedEvent(changeRequest.Id, changeRequest.Title));

        return changeRequest;
    }

    private static bool DetermineIfCABApprovalRequired(ChangeType changeType, RiskLevel riskLevel)
    {
        // Standard changes don't need CAB approval
        if (changeType == ChangeType.Standard)
            return false;

        // Emergency changes might bypass CAB (depends on policy)
        if (changeType == ChangeType.Emergency && riskLevel == RiskLevel.Low)
            return false;

        // Normal changes always need CAB
        return true;
    }

    // Update methods
    public void UpdateDetails(string title, string description)
    {
        if (Status != ChangeRequestStatus.Draft)
            throw new InvalidOperationException("Can only update details while in Draft status");

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required", nameof(title));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required", nameof(description));

        Title = title;
        Description = description;
    }

    public void UpdateAssessment(string impactAssessment, string rollbackPlan, string affectedSystems)
    {
        if (Status != ChangeRequestStatus.Draft)
            throw new InvalidOperationException("Can only update assessment while in Draft status");

        ImpactAssessment = impactAssessment;
        RollbackPlan = rollbackPlan;
        AffectedSystems = affectedSystems;
    }

    public void UpdateRisk(RiskLevel riskLevel)
    {
        if (Status != ChangeRequestStatus.Draft && Status != ChangeRequestStatus.UnderReview)
            throw new InvalidOperationException("Can only update risk during Draft or Review");

        RiskLevel = riskLevel;
        RequiresCABApproval = DetermineIfCABApprovalRequired(ChangeType, riskLevel);
    }

    public void UpdateSchedule(DateTime? scheduledStartDate, DateTime? scheduledEndDate, string? changeWindow)
    {
        if (scheduledStartDate.HasValue && scheduledEndDate.HasValue &&
            scheduledStartDate.Value >= scheduledEndDate.Value)
        {
            throw new ArgumentException("Start date must be before end date");
        }

        ScheduledStartDate = scheduledStartDate;
        ScheduledEndDate = scheduledEndDate;
        ChangeWindow = changeWindow;
    }

    // Workflow methods
    public void Submit()
    {
        if (Status != ChangeRequestStatus.Draft)
            throw new InvalidOperationException("Can only submit change requests in Draft status");

        Status = ChangeRequestStatus.Submitted;
        SubmittedDate = DateTime.UtcNow;

        RegisterDomainEvent(new ChangeRequestSubmittedEvent(Id, Title, RequiresCABApproval));
    }

    public void StartReview(Guid reviewedByUserId)
    {
        if (Status != ChangeRequestStatus.Submitted)
            throw new InvalidOperationException("Can only start review on submitted change requests");

        Status = ChangeRequestStatus.UnderReview;
        ReviewedByUserId = reviewedByUserId;
        ReviewStartedDate = DateTime.UtcNow;
    }

    public void Approve(Guid approvedByUserId, string? notes = null)
    {
        if (Status != ChangeRequestStatus.UnderReview && Status != ChangeRequestStatus.Submitted)
            throw new InvalidOperationException("Can only approve change requests that are submitted or under review");

        Status = ChangeRequestStatus.Approved;
        ApprovedByUserId = approvedByUserId;
        ApprovalNotes = notes;
        ApprovedDate = DateTime.UtcNow;

        RegisterDomainEvent(new ChangeRequestApprovedEvent(Id, Title, ApprovedByUserId.Value));
    }

    public void Deny(Guid deniedByUserId, string reason)
    {
        if (Status != ChangeRequestStatus.UnderReview && Status != ChangeRequestStatus.Submitted)
            throw new InvalidOperationException("Can only deny change requests that are submitted or under review");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Denial reason is required", nameof(reason));

        Status = ChangeRequestStatus.Denied;
        ApprovedByUserId = deniedByUserId; // Track who made the decision
        DenialReason = reason;
        DeniedDate = DateTime.UtcNow;

        RegisterDomainEvent(new ChangeRequestDeniedEvent(Id, Title, reason));
    }

    public void Schedule(DateTime scheduledStartDate, DateTime scheduledEndDate, string changeWindow)
    {
        if (Status != ChangeRequestStatus.Approved)
            throw new InvalidOperationException("Can only schedule approved change requests");

        if (scheduledStartDate >= scheduledEndDate)
            throw new ArgumentException("Start date must be before end date");

        ScheduledStartDate = scheduledStartDate;
        ScheduledEndDate = scheduledEndDate;
        ChangeWindow = changeWindow;
        Status = ChangeRequestStatus.Scheduled;
        ScheduledDate = DateTime.UtcNow;

        RegisterDomainEvent(new ChangeRequestScheduledEvent(Id, Title, scheduledStartDate, scheduledEndDate));
    }

    public void StartExecution()
    {
        if (Status != ChangeRequestStatus.Scheduled)
            throw new InvalidOperationException("Can only start execution on scheduled change requests");

        Status = ChangeRequestStatus.InProgress;
        ActualStartDate = DateTime.UtcNow;

        RegisterDomainEvent(new ChangeRequestStartedEvent(Id, Title));
    }

    public void Complete(string? implementationNotes = null)
    {
        if (Status != ChangeRequestStatus.InProgress)
            throw new InvalidOperationException("Can only complete change requests that are in progress");

        Status = ChangeRequestStatus.Completed;
        ActualEndDate = DateTime.UtcNow;
        ImplementationNotes = implementationNotes;
        CompletedDate = DateTime.UtcNow;

        RegisterDomainEvent(new ChangeRequestCompletedEvent(Id, Title));
    }

    public void MarkAsFailed(string reason)
    {
        if (Status != ChangeRequestStatus.InProgress)
            throw new InvalidOperationException("Can only mark in-progress change requests as failed");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Failure reason is required", nameof(reason));

        Status = ChangeRequestStatus.Failed;
        ActualEndDate = DateTime.UtcNow;
        ImplementationNotes = reason;
        FailedDate = DateTime.UtcNow;

        RegisterDomainEvent(new ChangeRequestFailedEvent(Id, Title, reason));
    }

    public void Rollback(string rollbackReason)
    {
        if (Status != ChangeRequestStatus.Failed && Status != ChangeRequestStatus.InProgress)
            throw new InvalidOperationException("Can only rollback failed or in-progress changes");

        if (string.IsNullOrWhiteSpace(rollbackReason))
            throw new ArgumentException("Rollback reason is required", nameof(rollbackReason));

        Status = ChangeRequestStatus.RolledBack;
        RollbackReason = rollbackReason;
        RolledBackDate = DateTime.UtcNow;

        RegisterDomainEvent(new ChangeRequestRolledBackEvent(Id, Title, rollbackReason));
    }

    public void Cancel()
    {
        if (Status == ChangeRequestStatus.Completed ||
            Status == ChangeRequestStatus.InProgress ||
            Status == ChangeRequestStatus.RolledBack)
        {
            throw new InvalidOperationException("Cannot cancel completed, in-progress, or rolled-back change requests");
        }

        Status = ChangeRequestStatus.Cancelled;
    }

    public void UpdateImplementationNotes(string notes)
    {
        if (Status != ChangeRequestStatus.InProgress)
            throw new InvalidOperationException("Can only update implementation notes during execution");

        ImplementationNotes = string.IsNullOrWhiteSpace(ImplementationNotes)
            ? notes
            : $"{ImplementationNotes}\n\n{DateTime.UtcNow:yyyy-MM-dd HH:mm}: {notes}";
    }

    public void LinkToProject(Guid projectId)
    {
        ProjectId = projectId;
    }

    // Helper methods
    public bool IsOverdue()
    {
        if (!ScheduledEndDate.HasValue || Status == ChangeRequestStatus.Completed)
            return false;

        return DateTime.UtcNow > ScheduledEndDate.Value;
    }

    public int? GetDaysUntilScheduled()
    {
        if (!ScheduledStartDate.HasValue)
            return null;

        var days = (ScheduledStartDate.Value - DateTime.UtcNow).Days;
        return days < 0 ? 0 : days;
    }
}
