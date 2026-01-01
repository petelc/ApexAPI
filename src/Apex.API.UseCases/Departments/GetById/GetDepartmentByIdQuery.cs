using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Departments.GetById;

public record GetDepartmentByIdQuery(DepartmentId DepartmentId) : IRequest<Result<DepartmentDto>>;