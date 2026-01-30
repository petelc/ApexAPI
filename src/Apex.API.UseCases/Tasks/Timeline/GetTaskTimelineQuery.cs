using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Tasks.Timeline;

/// <summary>
/// Query to get task activity timeline
/// </summary>
public record GetTaskTimelineQuery(
    TaskId TaskId
) : IRequest<Result<List<TaskActivityDto>>>;

/// <summary>
/// DTO for activity log entry
/// </summary>
public record TaskActivityDto(
    Guid Id,
    string ActivityType,
    string Description,
    string? Details,
    Guid UserId,
    DateTime Timestamp,
    
    // User lookup (enriched in Web layer)
    string? UserName = null,
    string? UserEmail = null
);
