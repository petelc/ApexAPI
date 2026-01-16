using Apex.API.UseCases.Users.DTOs;

namespace Apex.API.UseCases.ProjectRequests.DTOs;

/// <summary>
/// Project Request DTO with user information - ENHANCED with all aggregate properties
/// </summary>
public sealed record ProjectRequestDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string BusinessJustification { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    
    // User Information - Created By (Requesting User)
    public Guid RequestingUserId { get; init; }
    public UserSummaryDto? RequestingUser { get; init; }
    
    // User Information - Assigned To (NEW!)
    public Guid? AssignedToUserId { get; init; }
    public UserSummaryDto? AssignedToUser { get; init; }
    
    // User Information - Reviewed By
    public Guid? ReviewedByUserId { get; init; }
    public UserSummaryDto? ReviewedByUser { get; init; }
    public string? ReviewComments { get; init; }
    
    // User Information - Approved By (NEW!)
    public Guid? ApprovedByUserId { get; init; }
    public UserSummaryDto? ApprovedByUser { get; init; }
    public string? ApprovalNotes { get; init; }
    
    // User Information - Denied By
    public Guid? DeniedByUserId { get; init; }
    public UserSummaryDto? DeniedByUser { get; init; }
    public string? DenialReason { get; init; }
    
    // User Information - Converted By (NEW!)
    public Guid? ConvertedByUserId { get; init; }
    public UserSummaryDto? ConvertedByUser { get; init; }
    
    // Project Link
    public Guid? ProjectId { get; init; }
    
    // Budget & Timeline
    public decimal? EstimatedBudget { get; init; }
    public DateTime? ProposedStartDate { get; init; }
    public DateTime? ProposedEndDate { get; init; }
    public DateTime? DueDate { get; init; }  // NEW!
    
    // Dates
    public DateTime CreatedDate { get; init; }
    public DateTime? SubmittedDate { get; init; }
    public DateTime? ReviewedDate { get; init; }
    public DateTime? ApprovedDate { get; init; }  // NEW!
    public DateTime? DeniedDate { get; init; }
    public DateTime? ConvertedDate { get; init; }  // NEW!
    public DateTime? LastModifiedDate { get; init; }
}

/// <summary>
/// Simplified Project Request DTO for lists
/// </summary>
public sealed record ProjectRequestSummaryDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    
    // User Information
    public Guid RequestingUserId { get; init; }
    public string RequestingUserName { get; init; } = string.Empty;
    
    public Guid? AssignedToUserId { get; init; }
    public string? AssignedToUserName { get; init; }
    
    public DateTime CreatedDate { get; init; }
    public DateTime? DueDate { get; init; }
}
