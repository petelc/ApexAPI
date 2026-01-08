using MediatR;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.ChangeRequestAggregate;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.ChangeRequests.Reports.GetChangeMetrics;

/// <summary>
/// Get overall change management metrics
/// </summary>
public record GetChangeMetricsQuery(
    TenantId TenantId,
    DateTime? StartDate = null,
    DateTime? EndDate = null) : IRequest<ChangeMetricsResponse>;

public class GetChangeMetricsHandler : IRequestHandler<GetChangeMetricsQuery, ChangeMetricsResponse>
{
    private readonly IReadRepository<ChangeRequest> _repository;

    public GetChangeMetricsHandler(IReadRepository<ChangeRequest> repository)
    {
        _repository = repository;
    }

    public async Task<ChangeMetricsResponse> Handle(GetChangeMetricsQuery request, CancellationToken cancellationToken)
    {
        var allChanges = await _repository.ListAsync(cancellationToken);
        
        // Filter by tenant and date range
        var changes = allChanges
            .Where(c => c.TenantId == request.TenantId)
            .Where(c => !request.StartDate.HasValue || c.CreatedDate >= request.StartDate.Value)
            .Where(c => !request.EndDate.HasValue || c.CreatedDate <= request.EndDate.Value)
            .ToList();

        var totalChanges = changes.Count;
        if (totalChanges == 0)
        {
            return new ChangeMetricsResponse();
        }

        var completed = changes.Count(c => c.Status == ChangeRequestStatus.Completed);
        var failed = changes.Count(c => c.Status == ChangeRequestStatus.Failed);
        var rolledBack = changes.Count(c => c.Status == ChangeRequestStatus.RolledBack);
        var inProgress = changes.Count(c => c.Status == ChangeRequestStatus.InProgress);
        var scheduled = changes.Count(c => c.Status == ChangeRequestStatus.Scheduled);
        var pendingApproval = changes.Count(c => c.Status == ChangeRequestStatus.UnderReview);

        var approved = changes.Count(c => c.Status == ChangeRequestStatus.Approved || 
                                          c.Status == ChangeRequestStatus.Scheduled ||
                                          c.Status == ChangeRequestStatus.InProgress ||
                                          c.Status == ChangeRequestStatus.Completed);

        // Calculate success rate (completed / (completed + failed + rolled back))
        var finalizedChanges = completed + failed + rolledBack;
        var successRate = finalizedChanges > 0 
            ? (decimal)completed / finalizedChanges * 100 
            : 0;

        var rollbackRate = finalizedChanges > 0 
            ? (decimal)rolledBack / finalizedChanges * 100 
            : 0;

        var approvalRate = totalChanges > 0 
            ? (decimal)approved / totalChanges * 100 
            : 0;

        // Calculate average completion time
        var completedChanges = changes
            .Where(c => c.Status == ChangeRequestStatus.Completed && 
                       c.ActualStartDate.HasValue && 
                       c.ActualEndDate.HasValue)
            .ToList();

        var avgCompletionTime = completedChanges.Any()
            ? completedChanges.Average(c => (c.ActualEndDate!.Value - c.ActualStartDate!.Value).TotalHours)
            : 0;

        // Calculate average approval time
        var approvedChanges = changes
            .Where(c => c.SubmittedDate.HasValue && c.ApprovedDate.HasValue)
            .ToList();

        var avgApprovalTime = approvedChanges.Any()
            ? approvedChanges.Average(c => (c.ApprovedDate!.Value - c.SubmittedDate!.Value).TotalHours)
            : 0;

        return new ChangeMetricsResponse
        {
            TotalChanges = totalChanges,
            CompletedChanges = completed,
            FailedChanges = failed,
            RolledBackChanges = rolledBack,
            InProgressChanges = inProgress,
            ScheduledChanges = scheduled,
            PendingApprovalChanges = pendingApproval,
            
            SuccessRate = Math.Round(successRate, 2),
            RollbackRate = Math.Round(rollbackRate, 2),
            ApprovalRate = Math.Round(approvalRate, 2),
            
            AverageCompletionTimeHours = Math.Round(avgCompletionTime, 2),
            AverageApprovalTimeHours = Math.Round(avgApprovalTime, 2),
            
            ByType = new ChangesByTypeBreakdown
            {
                Standard = changes.Count(c => c.ChangeType == ChangeType.Standard),
                Normal = changes.Count(c => c.ChangeType == ChangeType.Normal),
                Emergency = changes.Count(c => c.ChangeType == ChangeType.Emergency)
            },
            
            ByRisk = new ChangesByRiskBreakdown
            {
                Low = changes.Count(c => c.RiskLevel == RiskLevel.Low),
                Medium = changes.Count(c => c.RiskLevel == RiskLevel.Medium),
                High = changes.Count(c => c.RiskLevel == RiskLevel.High),
                Critical = changes.Count(c => c.RiskLevel == RiskLevel.Critical)
            },
            
            ByPriority = new ChangesByPriorityBreakdown
            {
                Low = changes.Count(c => c.Priority == Priority.Low),
                Medium = changes.Count(c => c.Priority == Priority.Medium),
                High = changes.Count(c => c.Priority == Priority.High),
                Critical = changes.Count(c => c.Priority == Priority.Critical)
            }
        };
    }
}
