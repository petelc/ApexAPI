using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Departments.Create;

public record CreateDepartmentCommand(
    string Name,
    string Description,
    Guid? DepartmentManagerUserId = null
) : IRequest<Result<DepartmentId>>;