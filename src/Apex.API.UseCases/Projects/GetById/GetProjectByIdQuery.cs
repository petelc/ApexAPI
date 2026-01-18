using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;
using Apex.API.UseCases.Projects.DTOs;

namespace Apex.API.UseCases.Projects.GetById;

public record GetProjectByIdQuery(ProjectId ProjectId) : IRequest<Result<ProjectDto>>;