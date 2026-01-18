using Apex.API.Core.ValueObjects;
using Apex.API.UseCases.ProjectRequests.DTOs;
using Ardalis.Result;
using MediatR;

namespace Apex.API.UseCases.ProjectRequests.GetById;

/// <summary>
/// Query to get a single project request by ID
/// </summary>
public record GetProjectRequestByIdQuery(ProjectRequestId ProjectRequestId) 
    : IRequest<Result<ProjectRequestDto>>;
