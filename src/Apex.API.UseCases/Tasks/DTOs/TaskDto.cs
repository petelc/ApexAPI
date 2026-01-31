using Apex.API.UseCases.Users.DTOs;

namespace Apex.API.UseCases.Tasks.DTOs;

/// <summary>
/// Task Data Transfer Object
/// ✅ UPDATED: Added department assignment fields, notes, and additional user tracking
/// </summary>
public record TaskDto(
    Guid Id,
    string Title,
    string Description,
    string Status,
    string Priority,
    Guid ProjectId,

    // User Assignment
    Guid? AssignedToUserId,

    // ✅ NEW: Department Assignment
    Guid? AssignedToDepartmentId,
    string? AssignedToDepartmentName,

    // ✅ NEW: Notes
    string? ImplementationNotes,
    string? ResolutionNotes,

    // Time Tracking
    decimal? EstimatedHours,
    decimal? ActualHours,

    // Dates
    DateTime? DueDate,
    DateTime CreatedDate,
    DateTime? StartedDate,
    DateTime? CompletedDate,
    DateTime? LastModifiedDate,

    // Blocking
    string? BlockedReason,
    DateTime? BlockedDate,

    // User Tracking
    Guid CreatedByUserId,
    Guid? StartedByUserId,      // ✅ NEW
    Guid? CompletedByUserId,    // ✅ NEW

    // User lookup fields (enriched in Web layer)
    UserSummaryDto? CreatedByUser = null,
    UserSummaryDto? AssignedToUser = null,
    UserSummaryDto? StartedByUser = null,      // ✅ NEW
    UserSummaryDto? CompletedByUser = null     // ✅ NEW
);
