using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Projects.Complete;

public record CompleteProjectCommand(ProjectId ProjectId) : IRequest<Result>;