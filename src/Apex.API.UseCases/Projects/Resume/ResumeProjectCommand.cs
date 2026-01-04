using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Projects.Resume;

/// <summary>
/// Command to resume a project from on-hold status
/// </summary>
public record ResumeProjectCommand(
    ProjectId ProjectId
) : IRequest<Result>;
