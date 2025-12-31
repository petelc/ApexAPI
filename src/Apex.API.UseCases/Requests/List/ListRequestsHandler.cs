using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.RequestAggregate;
using Apex.API.Core.Interfaces;
using Apex.API.Core.ValueObjects;
using Apex.API.UseCases.Requests.GetById;

namespace Apex.API.UseCases.Requests.List;

/// <summary>
/// Handler for listing requests with filtering and pagination
/// </summary>
public class ListRequestsHandler : IRequestHandler<ListRequestsQuery, Result<PagedRequestList>>
{
    private readonly IReadRepository<Request> _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<ListRequestsHandler> _logger;

    public ListRequestsHandler(
        IReadRepository<Request> repository,
        ITenantContext tenantContext,
        ILogger<ListRequestsHandler> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<PagedRequestList>> Handle(
        ListRequestsQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get all requests for current tenant
            var allRequests = await _repository.ListAsync(cancellationToken);

            // Filter by tenant (security)
            var requests = allRequests
                .Where(r => r.TenantId == _tenantContext.CurrentTenantId)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                if (RequestStatus.TryFromName(query.Status, out var status))
                {
                    requests = requests.Where(r => r.Status == status);
                }
            }

            if (!string.IsNullOrWhiteSpace(query.Priority))
            {
                if (RequestPriority.TryFromName(query.Priority, out var priority))
                {
                    requests = requests.Where(r => r.Priority == priority);
                }
            }

            if (query.AssignedToUserId.HasValue)
            {
                requests = requests.Where(r => r.AssignedToUserId == query.AssignedToUserId.Value);
            }

            if (query.CreatedByUserId.HasValue)
            {
                requests = requests.Where(r => r.CreatedByUserId == query.CreatedByUserId.Value);
            }

            if (query.IsOverdue.HasValue && query.IsOverdue.Value)
            {
                requests = requests.Where(r => r.IsOverdue());
            }

            // Get total count
            var totalCount = requests.Count();

            // Apply pagination
            var skip = (query.PageNumber - 1) * query.PageSize;
            var pagedRequests = requests
                .OrderByDescending(r => r.CreatedDate)
                .Skip(skip)
                .Take(query.PageSize)
                .ToList();

            // Map to DTOs
            var dtos = pagedRequests.Select(r => new RequestDto(
                r.Id.Value,
                r.Title,
                r.Description,
                r.Status.Name,
                r.Priority.Name,
                r.CreatedByUserId,
                r.AssignedToUserId,
                r.ApprovedByUserId,
                r.CreatedDate,
                r.SubmittedDate,
                r.ApprovedDate,
                r.CompletedDate,
                r.DueDate,
                r.ApprovalNotes,
                r.DenialReason,
                r.IsOverdue(),
                r.GetDaysUntilDue()
            )).ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

            var result = new PagedRequestList(
                dtos,
                totalCount,
                query.PageNumber,
                query.PageSize,
                totalPages);

            _logger.LogInformation(
                "Listed requests: Count={Count}, Page={PageNumber}, TenantId={TenantId}",
                totalCount,
                query.PageNumber,
                _tenantContext.CurrentTenantId);

            return Result<PagedRequestList>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing requests");
            return Result<PagedRequestList>.Error("An error occurred while listing requests.");
        }
    }
}
