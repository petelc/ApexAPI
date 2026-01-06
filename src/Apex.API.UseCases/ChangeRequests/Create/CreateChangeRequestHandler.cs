using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Interfaces;
using Apex.API.Core.Aggregates.ChangeRequestAggregate;
using Apex.API.Core.ValueObjects;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.UseCases.ChangeRequests.Create;
/// <summary>
/// Handler for creating a new ChangeRequest
/// </summary>
public class CreateChangeRequestHandler : IRequestHandler<CreateChangeRequestCommand, Result<ChangeRequestId>>
{
    private readonly IRepository<ChangeRequest> _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateChangeRequestHandler> _logger;

    public CreateChangeRequestHandler(
        IRepository<ChangeRequest> repository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        ILogger<CreateChangeRequestHandler> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<ChangeRequestId>> Handle(
        CreateChangeRequestCommand command,
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
                    return Result<ChangeRequestId>.Error($"Invalid priority: {command.Priority}. Valid values: Low, Medium, High, Urgent");
                }
                priority = parsedPriority;
            }

            // Parse ChangeType
            ChangeType changeType = ChangeType.Standard;
            if (!ChangeType.TryFromName(command.ChangeType, out var parsedChangeType))
            {
                return Result<ChangeRequestId>.Error($"Invalid change type: {command.ChangeType}. Valid values: Standard, Emergency, Major");
            }
            changeType = parsedChangeType;

            RiskLevel riskLevel = RiskLevel.Medium;
            if (!RiskLevel.TryFromName(command.RiskLevel, out var parsedRiskLevel))
            {
                return Result<ChangeRequestId>.Error($"Invalid risk level: {command.RiskLevel}. Valid values: Low, Medium, High");
            }

            // Create ChangeRequest using factory method
            var changeRequest = ChangeRequest.Create(
                _tenantContext.CurrentTenantId,
                command.Title,
                command.Description,
                _currentUserService.UserId,
                changeType,
                priority,
                riskLevel,
                command.ImpactAssessment,
                command.RollbackPlan,
                command.AffectedSystems,
                command.ScheduledStartDate,
                command.ScheduledEndDate,
                command.ChangeWindow);

            // Save to database (domain events dispatched automatically)
            await _repository.AddAsync(changeRequest, cancellationToken);

            _logger.LogInformation(
                "ChangeRequest created successfully: ChangeRequestId={ChangeRequestId}, Title={Title}",
                changeRequest.Id,
                changeRequest.Title);

            return Result<ChangeRequestId>.Success(changeRequest.Id);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(
                ex,
                "Validation error creating ProjectRequest: {Message}",
                ex.Message);

            return Result<ChangeRequestId>.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error creating ProjectRequest: Title={Title}",
                command.Title);

            return Result<ChangeRequestId>.Error("An unexpected error occurred while creating the ChangeRequest.");
        }
    }
}
