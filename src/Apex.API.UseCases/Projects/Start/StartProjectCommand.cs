using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Projects.Start;

public record StartProjectCommand(ProjectId ProjectId) : IRequest<Result>;