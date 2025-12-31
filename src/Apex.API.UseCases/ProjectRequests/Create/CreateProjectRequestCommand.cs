using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.ProjectRequests.Create;

/// <summary>
/// Command to create a new ProjectRequest
/// </summary>
public record CreateProjectRequestCommand(
    string Title,
    string Description,
    string? Priority = null,
    DateTime? DueDate = null
) : IRequest<Result<ProjectRequestId>>;
