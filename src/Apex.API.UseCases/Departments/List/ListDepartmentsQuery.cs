using MediatR;
using Ardalis.Result;
using Apex.API.UseCases.Departments.GetById;

namespace Apex.API.UseCases.Departments.List;

public record ListDepartmentsQuery(
    bool? IsActive = null
) : IRequest<Result<List<DepartmentDto>>>;