using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Requests.Assign;

/// <summary>
/// Command to assign a request to a user (Approved → InProgress)
/// </summary>
public record AssignRequestCommand(
    RequestId RequestId,
    Guid AssignedToUserId
) : IRequest<Result>;
