using Apex.API.UseCases.Users.DTOs;

namespace Apex.API.UseCases.Tasks.DTOs;

public record TaskDto(
    Guid Id,
    string Title,
    string Description,
    string Status,
    string Priority,
    Guid ProjectId,
    Guid? AssignedToUserId,
    decimal? EstimatedHours,
    decimal? ActualHours,
    DateTime? DueDate,
    DateTime CreatedDate,
    DateTime? StartedDate,
    DateTime? CompletedDate,
    DateTime? LastModifiedDate,
    string? BlockedReason,
    DateTime? BlockedDate,
    Guid CreatedByUserId,

    // User lookup fields
    UserSummaryDto? CreatedByUser = null,
    UserSummaryDto? AssignedToUser = null
);