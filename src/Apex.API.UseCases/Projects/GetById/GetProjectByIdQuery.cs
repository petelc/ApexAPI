using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Projects.GetById;

public record GetProjectByIdQuery(ProjectId ProjectId) : IRequest<Result<ProjectDto>>;