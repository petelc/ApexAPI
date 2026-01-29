using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Users.AssignDepartment;

public record AssignDepartmentToUserCommand(
    Guid UserId,
    DepartmentId? DepartmentId  // Nullable - allows removing department assignment
) : IRequest<Result>;
