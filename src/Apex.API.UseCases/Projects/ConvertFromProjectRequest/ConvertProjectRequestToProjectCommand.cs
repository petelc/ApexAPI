using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Projects.ConvertFromProjectRequest;

/// <summary>
/// Command to convert an approved ProjectRequest into a Project
/// This is the main workflow: ProjectRequest (Approved) → Project (Planning)
/// </summary>
public record ConvertProjectRequestToProjectCommand(
    ProjectRequestId ProjectRequestId,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    decimal? Budget = null,
    Guid? ProjectManagerUserId = null
) : IRequest<Result<ProjectId>>;