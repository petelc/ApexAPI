using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

public record AssignTaskToDepartmentCommand(
    TaskId TaskId,
    DepartmentId DepartmentId
) : IRequest<Result>;