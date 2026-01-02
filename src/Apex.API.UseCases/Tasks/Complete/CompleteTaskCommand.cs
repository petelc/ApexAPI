using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Tasks.Complete;

public record CompleteTaskCommand(TaskId TaskId) : IRequest<Result>;
