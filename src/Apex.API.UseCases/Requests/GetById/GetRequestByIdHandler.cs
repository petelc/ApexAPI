using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.RequestAggregate;
using Apex.API.Core.Interfaces;

namespace Apex.API.UseCases.Requests.GetById;

/// <summary>
/// Handler for getting a request by ID
/// </summary>
public class GetRequestByIdHandler : IRequestHandler<GetRequestByIdQuery, Result<RequestDto>>
{
    private readonly IReadRepository<Request> _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetRequestByIdHandler> _logger;

    public GetRequestByIdHandler(
        IReadRepository<Request> repository,
        ITenantContext tenantContext,
        ILogger<GetRequestByIdHandler> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<RequestDto>> Handle(
        GetRequestByIdQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = await _repository.GetByIdAsync(query.RequestId, cancellationToken);

            if (request == null)
            {
                _logger.LogWarning(
                    "Request not found: RequestId={RequestId}",
                    query.RequestId);

                return Result<RequestDto>.NotFound("Request not found.");
            }

            // Verify tenant ownership (multi-tenant security)
            if (request.TenantId != _tenantContext.CurrentTenantId)
            {
                _logger.LogWarning(
                    "Unauthorized access attempt: RequestId={RequestId}, TenantId={TenantId}",
                    query.RequestId,
                    _tenantContext.CurrentTenantId);

                return Result<RequestDto>.Forbidden();
            }

            var dto = new RequestDto(
                request.Id.Value,
                request.Title,
                request.Description,
                request.Status.Name,
                request.Priority.Name,
                request.CreatedByUserId,
                request.AssignedToUserId,
                request.ApprovedByUserId,
                request.CreatedDate,
                request.SubmittedDate,
                request.ApprovedDate,
                request.CompletedDate,
                request.DueDate,
                request.ApprovalNotes,
                request.DenialReason,
                request.IsOverdue(),
                request.GetDaysUntilDue());

            return Result<RequestDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving request: RequestId={RequestId}",
                query.RequestId);

            return Result<RequestDto>.Error("An error occurred while retrieving the request.");
        }
    }
}
