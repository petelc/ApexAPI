using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Tasks.Start;

public record StartTaskCommand(TaskId TaskId) : IRequest<Result>;