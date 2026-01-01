using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.DepartmentAggregate;
using Apex.API.Core.Aggregates.UserAggregate;
using Apex.API.Core.Interfaces;
using Apex.API.UseCases.Departments.GetById;

namespace Apex.API.UseCases.Departments.List;

public class ListDepartmentsHandler : IRequestHandler<ListDepartmentsQuery, Result<List<DepartmentDto>>>
{
    private readonly IReadRepository<Department> _repository;
    private readonly ITenantContext _tenantContext;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<ListDepartmentsHandler> _logger;

    public ListDepartmentsHandler(
        IReadRepository<Department> repository,
        ITenantContext tenantContext,
        UserManager<User> userManager,
        ILogger<ListDepartmentsHandler> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result<List<DepartmentDto>>> Handle(
        ListDepartmentsQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            var allDepartments = await _repository.ListAsync(cancellationToken);

            var departments = allDepartments
                .Where(d => d.TenantId == _tenantContext.CurrentTenantId)
                .AsQueryable();

            if (query.IsActive.HasValue)
            {
                departments = departments.Where(d => d.IsActive == query.IsActive.Value);
            }

            var departmentList = departments.OrderBy(d => d.Name).ToList();

            var dtos = departmentList.Select(d =>
            {
                string? managerName = null;
                if (d.DepartmentManagerUserId.HasValue)
                {
                    var manager = _userManager.FindByIdAsync(d.DepartmentManagerUserId.Value.ToString()).Result;
                    managerName = manager?.FullName;
                }

                var memberCount = _userManager.Users
                    .Count(u => u.DepartmentId == d.Id && u.TenantId == _tenantContext.CurrentTenantId);

                return new DepartmentDto(
                    d.Id.Value,
                    d.Name,
                    d.Description,
                    d.DepartmentManagerUserId,
                    managerName,
                    d.IsActive,
                    memberCount,
                    d.CreatedDate,
                    d.LastModifiedDate);
            }).ToList();

            return Result<List<DepartmentDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing departments");
            return Result<List<DepartmentDto>>.Error("An error occurred while listing departments.");
        }
    }
}