using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Interfaces;
using Apex.API.Core.Aggregates.RequestAggregate;
using Apex.API.Core.ValueObjects;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.UseCases.Requests.Create;

/// <summary>
/// Handler for creating a new request
/// </summary>
public class CreateRequestHandler : IRequestHandler<CreateRequestCommand, Result<RequestId>>
{
    private readonly IRepository<Request> _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateRequestHandler> _logger;

    public CreateRequestHandler(
        IRepository<Request> repository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        ILogger<CreateRequestHandler> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<RequestId>> Handle(
        CreateRequestCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Creating request: Title={Title}, User={UserId}, Tenant={TenantId}",
                command.Title,
                _currentUserService.UserId,
                _tenantContext.CurrentTenantId);

            // Parse priority if provided
            RequestPriority priority = RequestPriority.Medium;
            if (!string.IsNullOrWhiteSpace(command.Priority))
            {
                if (!RequestPriority.TryFromName(command.Priority, out var parsedPriority))
                {
                    return Result<RequestId>.Error($"Invalid priority: {command.Priority}. Valid values: Low, Medium, High, Urgent");
                }
                priority = parsedPriority;
            }

            // Create request using factory method
            var request = Request.Create(
                _tenantContext.CurrentTenantId,
                command.Title,
                command.Description,
                _currentUserService.UserId,
                priority,
                command.DueDate);

            // Save to database (domain events dispatched automatically)
            await _repository.AddAsync(request, cancellationToken);

            _logger.LogInformation(
                "Request created successfully: RequestId={RequestId}, Title={Title}",
                request.Id,
                request.Title);

            return Result<RequestId>.Success(request.Id);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(
                ex,
                "Validation error creating request: {Message}",
                ex.Message);

            return Result<RequestId>.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error creating request: Title={Title}",
                command.Title);

            return Result<RequestId>.Error("An unexpected error occurred while creating the request.");
        }
    }
}
