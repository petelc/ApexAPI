using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Interfaces;
using Apex.API.Core.Aggregates.ProjectRequestAggregate;
using Apex.API.Core.ValueObjects;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.UseCases.ProjectRequests.Create;

/// <summary>
/// Handler for creating a new ProjectRequest
/// </summary>
public class CreateProjectRequestHandler : IRequestHandler<CreateProjectRequestCommand, Result<ProjectRequestId>>
{
    private readonly IRepository<ProjectRequest> _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateProjectRequestHandler> _logger;

    public CreateProjectRequestHandler(
        IRepository<ProjectRequest> repository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        ILogger<CreateProjectRequestHandler> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<ProjectRequestId>> Handle(
        CreateProjectRequestCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Creating ProjectRequest: Title={Title}, User={UserId}, Tenant={TenantId}",
                command.Title,
                _currentUserService.UserId,
                _tenantContext.CurrentTenantId);

            // Parse priority if provided
            RequestPriority priority = RequestPriority.Medium;
            if (!string.IsNullOrWhiteSpace(command.Priority))
            {
                if (!RequestPriority.TryFromName(command.Priority, out var parsedPriority))
                {
                    return Result<ProjectRequestId>.Error($"Invalid priority: {command.Priority}. Valid values: Low, Medium, High, Urgent");
                }
                priority = parsedPriority;
            }

            // Create ProjectRequest using factory method
            var projectRequest = ProjectRequest.Create(
                _tenantContext.CurrentTenantId,
                command.Title,
                command.Description,
                _currentUserService.UserId,
                priority,
                command.DueDate);

            // Save to database (domain events dispatched automatically)
            await _repository.AddAsync(projectRequest, cancellationToken);

            _logger.LogInformation(
                "ProjectRequest created successfully: ProjectRequestId={ProjectRequestId}, Title={Title}",
                projectRequest.Id,
                projectRequest.Title);

            return Result<ProjectRequestId>.Success(projectRequest.Id);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(
                ex,
                "Validation error creating ProjectRequest: {Message}",
                ex.Message);

            return Result<ProjectRequestId>.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error creating ProjectRequest: Title={Title}",
                command.Title);

            return Result<ProjectRequestId>.Error("An unexpected error occurred while creating the ProjectRequest.");
        }
    }
}
