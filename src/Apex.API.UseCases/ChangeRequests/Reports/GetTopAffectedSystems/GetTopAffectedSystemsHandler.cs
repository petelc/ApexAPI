using MediatR;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.ChangeRequestAggregate;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.ChangeRequests.Reports.GetTopAffectedSystems;

/// <summary>
/// Get top affected systems by change frequency
/// </summary>
public record GetTopAffectedSystemsQuery(
    TenantId TenantId,
    int TopCount = 10,
    DateTime? StartDate = null,
    DateTime? EndDate = null) : IRequest<TopAffectedSystemsResponse>;

public class GetTopAffectedSystemsHandler : IRequestHandler<GetTopAffectedSystemsQuery, TopAffectedSystemsResponse>
{
    private readonly IReadRepository<ChangeRequest> _repository;

    public GetTopAffectedSystemsHandler(IReadRepository<ChangeRequest> repository)
    {
        _repository = repository;
    }

    public async Task<TopAffectedSystemsResponse> Handle(GetTopAffectedSystemsQuery request, CancellationToken cancellationToken)
    {
        var allChanges = await _repository.ListAsync(cancellationToken);
        
        // Filter by tenant and date range
        var changes = allChanges
            .Where(c => c.TenantId == request.TenantId)
            .Where(c => !request.StartDate.HasValue || c.CreatedDate >= request.StartDate.Value)
            .Where(c => !request.EndDate.HasValue || c.CreatedDate <= request.EndDate.Value)
            .Where(c => !string.IsNullOrWhiteSpace(c.AffectedSystems))
            .ToList();

        // Parse affected systems (comma-separated)
        var systemStats = new Dictionary<string, List<ChangeRequest>>();

        foreach (var change in changes)
        {
            var systems = change.AffectedSystems
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s));

            foreach (var system in systems)
            {
                if (!systemStats.ContainsKey(system))
                {
                    systemStats[system] = new List<ChangeRequest>();
                }
                systemStats[system].Add(change);
            }
        }

        // Calculate stats for each system
        var result = systemStats
            .Select(kvp =>
            {
                var systemChanges = kvp.Value;
                var successful = systemChanges.Count(c => c.Status == ChangeRequestStatus.Completed);
                var failed = systemChanges.Count(c => c.Status == ChangeRequestStatus.Failed || 
                                                      c.Status == ChangeRequestStatus.RolledBack);
                var total = successful + failed;
                var successRate = total > 0 ? (decimal)successful / total * 100 : 0;

                return new AffectedSystemStats
                {
                    SystemName = kvp.Key,
                    ChangeCount = systemChanges.Count,
                    SuccessfulChanges = successful,
                    FailedChanges = failed,
                    SuccessRate = Math.Round(successRate, 2),
                    LastChangeDate = systemChanges.Max(c => c.CreatedDate)
                };
            })
            .OrderByDescending(s => s.ChangeCount)
            .Take(request.TopCount)
            .ToList();

        return new TopAffectedSystemsResponse
        {
            Systems = result
        };
    }
}
