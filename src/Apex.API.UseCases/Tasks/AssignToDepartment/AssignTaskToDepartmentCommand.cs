using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Tasks.AssignToDepartment;

public record AssignTaskToDepartmentCommand(
    TaskId TaskId,
    DepartmentId DepartmentId
) : IRequest<Result>;