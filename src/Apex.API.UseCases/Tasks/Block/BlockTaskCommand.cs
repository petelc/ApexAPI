using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Tasks.Block;

public record BlockTaskCommand(TaskId TaskId, string Reason) : IRequest<Result>;