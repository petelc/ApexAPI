using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.ProjectAggregate;
using Apex.API.Core.Aggregates.UserAggregate;
using Apex.API.Core.Interfaces;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.UseCases.Projects.AssignProjectManager;

/// <summary>
/// Handler for assigning a project manager
/// </summary>
public class AssignProjectManagerHandler : IRequestHandler<AssignProjectManagerCommand, Result>
{
    private readonly IRepository<Project> _projectRepository;
    private readonly IReadRepository<User> _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<AssignProjectManagerHandler> _logger;

    public AssignProjectManagerHandler(
        IRepository<Project> projectRepository,
        IReadRepository<User> userRepository,
        ICurrentUserService currentUserService,
        ITenantContext tenantContext,
        ILogger<AssignProjectManagerHandler> logger)
    {
        _projectRepository = projectRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result> Handle(
        AssignProjectManagerCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var project = await _projectRepository.GetByIdAsync(command.ProjectId, cancellationToken);

            if (project == null)
            {
                _logger.LogWarning("Project not found: ProjectId={ProjectId}", command.ProjectId);
                return Result.NotFound("Project not found.");
            }

            // Verify tenant ownership
            if (project.TenantId != _tenantContext.CurrentTenantId)
            {
                _logger.LogWarning(
                    "Unauthorized access attempt: ProjectId={ProjectId}, TenantId={TenantId}",
                    command.ProjectId,
                    _tenantContext.CurrentTenantId);
                return Result.Forbidden();
            }

            // Verify user exists and belongs to tenant
            var users = await _userRepository.ListAsync(cancellationToken);
            var user = users.FirstOrDefault(u =>
                u.Id == command.ProjectManagerUserId &&
                u.TenantId == _tenantContext.CurrentTenantId);

            if (user == null)
            {
                _logger.LogWarning(
                    "User not found or doesn't belong to tenant: UserId={UserId}, TenantId={TenantId}",
                    command.ProjectManagerUserId,
                    _tenantContext.CurrentTenantId);
                return Result.NotFound("User not found or doesn't belong to this tenant.");
            }

            // Assign project manager
            project.AssignProjectManager(command.ProjectManagerUserId, _currentUserService.UserId);

            await _projectRepository.UpdateAsync(project, cancellationToken);

            _logger.LogInformation(
                "Project manager assigned: ProjectId={ProjectId}, ManagerUserId={ManagerUserId}",
                project.Id,
                command.ProjectManagerUserId);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot assign PM: {Message}", ex.Message);
            return Result.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error assigning project manager: ProjectId={ProjectId}",
                command.ProjectId);
            return Result.Error("An unexpected error occurred while assigning project manager.");
        }
    }
}
