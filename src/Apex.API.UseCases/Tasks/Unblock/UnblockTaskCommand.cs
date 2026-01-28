using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Tasks.Unblock;

/// <summary>
/// Command to unblock a task
/// </summary>
public record UnblockTaskCommand(TaskId TaskId) : IRequest<Result>;
