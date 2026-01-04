using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Projects.AssignProjectManager;

/// <summary>
/// Command to assign a project manager to a project
/// </summary>
public record AssignProjectManagerCommand(
    ProjectId ProjectId,
    Guid ProjectManagerUserId
) : IRequest<Result>;