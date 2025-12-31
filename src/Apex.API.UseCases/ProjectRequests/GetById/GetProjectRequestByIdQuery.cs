using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.ProjectRequests.GetById;

/// <summary>
/// Query to get a ProjectRequest by ID
/// </summary>
public record GetProjectRequestByIdQuery(ProjectRequestId ProjectRequestId)
    : IRequest<Result<ProjectRequestDto>>;


