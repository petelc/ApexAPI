/// <summary>
/// Task DTO for list response
/// </summary>
public record TaskDto(
    Guid Id,
    string Title,
    string Description,
    string Status,
    string Priority,
    Guid? AssignedToUserId,
    string? AssignedToUserName,
    Guid? AssignedToDepartmentId,
    string? AssignedToDepartmentName,
    decimal? EstimatedHours,  // ✅ Made nullable to match Task entity
    decimal ActualHours,
    DateTime CreatedDate,
    DateTime? StartedDate,
    DateTime? CompletedDate,
    string? BlockedReason
);