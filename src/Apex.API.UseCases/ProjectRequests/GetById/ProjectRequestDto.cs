namespace Apex.API.UseCases.ProjectRequests.GetById;

public record ProjectRequestDto(
    Guid Id,
    string Title,
    string Description,
    string Status,
    string Priority,
    Guid CreatedByUserId,
    Guid? ReviewedByUserId,
    Guid? ApprovedByUserId,
    Guid? ConvertedByUserId,
    DateTime CreatedDate,
    DateTime? SubmittedDate,
    DateTime? ReviewStartedDate,
    DateTime? ApprovedDate,
    DateTime? DeniedDate,
    DateTime? ConvertedDate,     // ✅ This instead of CompletedDate
    DateTime? DueDate,
    string? ReviewNotes,
    string? ApprovalNotes,
    string? DenialReason,
    Guid? ProjectId,
    bool IsOverdue,
    int? DaysUntilDue);