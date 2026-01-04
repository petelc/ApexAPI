using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Tasks.AssignToUser;

public record AssignTaskToUserCommand(
    TaskId TaskId,
    Guid AssignedToUserId
) : IRequest<Result>;