using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Interfaces;
using Apex.API.Core.Aggregates.DepartmentAggregate;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Departments.Create;

public class CreateDepartmentHandler : IRequestHandler<CreateDepartmentCommand, Result<DepartmentId>>
{
    private readonly IRepository<Department> _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<CreateDepartmentHandler> _logger;

    public CreateDepartmentHandler(
        IRepository<Department> repository,
        ITenantContext tenantContext,
        ILogger<CreateDepartmentHandler> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<DepartmentId>> Handle(
        CreateDepartmentCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var department = Department.Create(
                _tenantContext.CurrentTenantId,
                command.Name,
                command.Description,
                command.DepartmentManagerUserId);

            await _repository.AddAsync(department, cancellationToken);

            _logger.LogInformation(
                "Department created: DepartmentId={DepartmentId}, Name={Name}",
                department.Id,
                department.Name);

            return Result<DepartmentId>.Success(department.Id);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error creating department: {Message}", ex.Message);
            return Result<DepartmentId>.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating department");
            return Result<DepartmentId>.Error("An error occurred while creating the department.");
        }
    }
}