using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.ChangeRequestAggregate;
using Apex.API.Core.Interfaces;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.UseCases.ChangeRequests.Schedule;

public class ScheduleChangeRequestHandler : IRequestHandler<ScheduleChangeRequestCommand, Result>
{
    private readonly IRepository<ChangeRequest> _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ScheduleChangeRequestHandler> _logger;

    public ScheduleChangeRequestHandler(
        IRepository<ChangeRequest> repository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        ILogger<ScheduleChangeRequestHandler> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> Handle(ScheduleChangeRequestCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var changeRequest = await _repository.GetByIdAsync(command.ChangeRequestId, cancellationToken);
            if (changeRequest == null)
                return Result.NotFound("ChangeRequest not found.");

            if (changeRequest.TenantId != _tenantContext.CurrentTenantId)
                return Result.Forbidden();

            changeRequest.Schedule(command.ScheduleStartDate, command.ScheduleEndDate, command.Window);

            await _repository.UpdateAsync(changeRequest, cancellationToken);

            _logger.LogInformation("ChangeRequest scheduled: ChangeRequestId={ChangeRequestId}", command.ChangeRequestId);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot schedule ChangeRequest: {Message}", ex.Message);
            return Result.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling ChangeRequest: ChangeRequestId={ChangeRequestId}", command.ChangeRequestId);
            return Result.Error("An error occurred while scheduling the ChangeRequest.");
        }
    }
}