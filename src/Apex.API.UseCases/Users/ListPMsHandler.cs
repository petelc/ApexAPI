using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Apex.API.Core.Aggregates.UserAggregate;
using Apex.API.Core.Interfaces;
using Apex.API.UseCases.Users.PMs;

namespace Apex.API.UseCases.Users.List;

/// <summary>
/// Handler for listing users in the current tenant
/// </summary>
public class ListPMsHandler : IRequestHandler<ListPMsQuery, Result<List<PMDto>>>
{
    private readonly UserManager<User> _userManager;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<ListPMsHandler> _logger;
    public ListPMsHandler(
        UserManager<User> userManager,
        ITenantContext tenantContext,
        ILogger<ListPMsHandler> logger)
    {
        _userManager = userManager;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<List<PMDto>>> Handle(
        ListPMsQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get all users for current tenant
            var users = _userManager.Users
                .Where(u => u.TenantId == _tenantContext.CurrentTenantId)
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToList();

            var pmDtos = new List<PMDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                pmDtos.Add(new PMDto(
                    user.Id,
                    user.Email!,
                    user.FirstName,
                    user.LastName,
                    user.FullName,
                    roles));
            }

            _logger.LogInformation(
                "Listed users: Count={Count}, TenantId={TenantId}",
                pmDtos.Count,
                _tenantContext.CurrentTenantId);

            return Result<List<PMDto>>.Success(pmDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing users");
            return Result<List<PMDto>>.Error("An error occurred while listing users.");
        }
    }
}
