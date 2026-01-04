using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Projects.Cancel;

/// <summary>
/// Command to cancel a project
/// </summary>
public record CancelProjectCommand(
    ProjectId ProjectId,
    string Reason
) : IRequest<Result>;
