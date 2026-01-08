using MediatR;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.ChangeRequestAggregate;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.ChangeRequests.Reports.GetSuccessRate;

/// <summary>
/// Get change success rate analysis
/// </summary>
public record GetSuccessRateQuery(
    TenantId TenantId,
    DateTime? StartDate = null,
    DateTime? EndDate = null) : IRequest<SuccessRateResponse>;

public class GetSuccessRateHandler : IRequestHandler<GetSuccessRateQuery, SuccessRateResponse>
{
    private readonly IReadRepository<ChangeRequest> _repository;

    public GetSuccessRateHandler(IReadRepository<ChangeRequest> repository)
    {
        _repository = repository;
    }

    public async Task<SuccessRateResponse> Handle(GetSuccessRateQuery request, CancellationToken cancellationToken)
    {
        var allChanges = await _repository.ListAsync(cancellationToken);
        
        // Filter by tenant and date range
        var changes = allChanges
            .Where(c => c.TenantId == request.TenantId)
            .Where(c => !request.StartDate.HasValue || c.CreatedDate >= request.StartDate.Value)
            .Where(c => !request.EndDate.HasValue || c.CreatedDate <= request.EndDate.Value)
            .Where(c => c.Status == ChangeRequestStatus.Completed || 
                       c.Status == ChangeRequestStatus.Failed || 
                       c.Status == ChangeRequestStatus.RolledBack)
            .ToList();

        var totalChanges = changes.Count;
        if (totalChanges == 0)
        {
            return new SuccessRateResponse();
        }

        var successful = changes.Count(c => c.Status == ChangeRequestStatus.Completed);
        var failed = changes.Count(c => c.Status == ChangeRequestStatus.Failed);
        var rolledBack = changes.Count(c => c.Status == ChangeRequestStatus.RolledBack);

        var response = new SuccessRateResponse
        {
            TotalChanges = totalChanges,
            SuccessfulChanges = successful,
            FailedChanges = failed,
            RolledBackChanges = rolledBack,
            
            SuccessPercentage = Math.Round((decimal)successful / totalChanges * 100, 2),
            FailurePercentage = Math.Round((decimal)failed / totalChanges * 100, 2),
            RollbackPercentage = Math.Round((decimal)rolledBack / totalChanges * 100, 2),
            
            ByType = new SuccessRateByType
            {
                Standard = CalculateTypeSuccessRate(changes, ChangeType.Standard),
                Normal = CalculateTypeSuccessRate(changes, ChangeType.Normal),
                Emergency = CalculateTypeSuccessRate(changes, ChangeType.Emergency)
            }
        };

        return response;
    }

    private static TypeSuccessRate CalculateTypeSuccessRate(List<ChangeRequest> changes, ChangeType type)
    {
        var typeChanges = changes.Where(c => c.ChangeType == type).ToList();
        var total = typeChanges.Count;
        
        if (total == 0)
        {
            return new TypeSuccessRate();
        }

        var successful = typeChanges.Count(c => c.Status == ChangeRequestStatus.Completed);
        var failed = typeChanges.Count(c => c.Status == ChangeRequestStatus.Failed || 
                                             c.Status == ChangeRequestStatus.RolledBack);

        return new TypeSuccessRate
        {
            Total = total,
            Successful = successful,
            Failed = failed,
            SuccessPercentage = Math.Round((decimal)successful / total * 100, 2)
        };
    }
}
