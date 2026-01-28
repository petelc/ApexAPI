using Apex.API.UseCases.Users.DTOs;

namespace Apex.API.UseCases.Tasks.DTOs;

/// <summary>
/// Task DTO with enhanced fields for implementation notes, resolution notes, and user tracking
/// </summary>
public record TaskDto(
    Guid Id,
    Guid ProjectId,
    string Title,
    string Description,
    string Status,
    string Priority,
    
    // ✅ NEW: Notes
    string? ImplementationNotes,
    string? ResolutionNotes,
    
    // Assignment
    Guid? AssignedToUserId,
    Guid? AssignedToDepartmentId,
    
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
    Guid? StartedByUserId,      // ✅ NEW: Who started the task
    Guid? CompletedByUserId,    // ✅ NEW: Who completed the task
    
    // User lookup fields (enriched in Web layer)
    UserSummaryDto? CreatedByUser = null,
    UserSummaryDto? AssignedToUser = null,
    UserSummaryDto? StartedByUser = null,      // ✅ NEW
    UserSummaryDto? CompletedByUser = null     // ✅ NEW
);
