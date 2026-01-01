using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.DepartmentAggregate;
using Apex.API.Core.Aggregates.UserAggregate;
using Apex.API.Core.Interfaces;

namespace Apex.API.UseCases.Departments.GetById;

public class GetDepartmentByIdHandler : IRequestHandler<GetDepartmentByIdQuery, Result<DepartmentDto>>
{
    private readonly IReadRepository<Department> _repository;
    private readonly ITenantContext _tenantContext;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<GetDepartmentByIdHandler> _logger;

    public GetDepartmentByIdHandler(
        IReadRepository<Department> repository,
        ITenantContext tenantContext,
        UserManager<User> userManager,
        ILogger<GetDepartmentByIdHandler> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result<DepartmentDto>> Handle(
        GetDepartmentByIdQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            var department = await _repository.GetByIdAsync(query.DepartmentId, cancellationToken);

            if (department == null)
                return Result<DepartmentDto>.NotFound("Department not found.");

            if (department.TenantId != _tenantContext.CurrentTenantId)
                return Result<DepartmentDto>.Forbidden();

            // Get manager name
            string? managerName = null;
            if (department.DepartmentManagerUserId.HasValue)
            {
                var manager = await _userManager.FindByIdAsync(
                    department.DepartmentManagerUserId.Value.ToString());
                managerName = manager?.FullName;
            }

            // Count members
            var memberCount = _userManager.Users
                .Count(u => u.DepartmentId == department.Id &&
                           u.TenantId == _tenantContext.CurrentTenantId);

            var dto = new DepartmentDto(
                department.Id.Value,
                department.Name,
                department.Description,
                department.DepartmentManagerUserId,
                managerName,
                department.IsActive,
                memberCount,
                department.CreatedDate,
                department.LastModifiedDate);

            return Result<DepartmentDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving department");
            return Result<DepartmentDto>.Error("An error occurred while retrieving the department.");
        }
    }
}