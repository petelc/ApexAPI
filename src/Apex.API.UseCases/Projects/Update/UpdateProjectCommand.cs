using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Projects.Update;

/// <summary>
/// Command to update project details
/// </summary>
public record UpdateProjectCommand(
    ProjectId ProjectId,
    string? Name = null,
    string? Description = null,
    decimal? Budget = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    string? Priority = null
) : IRequest<Result>;
