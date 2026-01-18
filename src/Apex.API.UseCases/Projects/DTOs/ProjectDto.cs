using Apex.API.UseCases.Users.DTOs;

namespace Apex.API.UseCases.Projects.DTOs;

public record ProjectDto(
    Guid Id,
    string Name,
    string Description,
    string Status,
    string Priority,
    Guid ProjectRequestId,
    decimal? Budget,
    DateTime? StartDate,
    DateTime? EndDate,
    DateTime? ActualStartDate,
    DateTime? ActualEndDate,
    Guid CreatedByUserId,
    Guid? ProjectManagerUserId,
    DateTime CreatedDate,
    DateTime? LastModifiedDate,
    bool IsOverdue,
    int? DaysUntilDeadline,
    int? DurationDays,

    // User lookup fields
    UserSummaryDto? CreatedByUser = null,
    UserSummaryDto? ProjectManagerUser = null
);
