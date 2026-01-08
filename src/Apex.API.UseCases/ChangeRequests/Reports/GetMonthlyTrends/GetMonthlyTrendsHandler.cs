using MediatR;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.ChangeRequestAggregate;
using Apex.API.Core.ValueObjects;
using System.Globalization;

namespace Apex.API.UseCases.ChangeRequests.Reports.GetMonthlyTrends;

/// <summary>
/// Get monthly trend analysis
/// </summary>
public record GetMonthlyTrendsQuery(
    TenantId TenantId,
    int MonthsBack = 12) : IRequest<MonthlyTrendsResponse>;

public class GetMonthlyTrendsHandler : IRequestHandler<GetMonthlyTrendsQuery, MonthlyTrendsResponse>
{
    private readonly IReadRepository<ChangeRequest> _repository;

    public GetMonthlyTrendsHandler(IReadRepository<ChangeRequest> repository)
    {
        _repository = repository;
    }

    public async Task<MonthlyTrendsResponse> Handle(GetMonthlyTrendsQuery request, CancellationToken cancellationToken)
    {
        var allChanges = await _repository.ListAsync(cancellationToken);
        
        var startDate = DateTime.UtcNow.AddMonths(-request.MonthsBack);
        
        // Filter by tenant and date range
        var changes = allChanges
            .Where(c => c.TenantId == request.TenantId)
            .Where(c => c.CreatedDate >= startDate)
            .ToList();

        var monthlyData = new List<MonthlyTrendData>();

        // Generate data for each month
        for (int i = request.MonthsBack - 1; i >= 0; i--)
        {
            var monthDate = DateTime.UtcNow.AddMonths(-i);
            var year = monthDate.Year;
            var month = monthDate.Month;

            var monthChanges = changes
                .Where(c => c.CreatedDate.Year == year && c.CreatedDate.Month == month)
                .ToList();

            var completed = monthChanges.Count(c => c.Status == ChangeRequestStatus.Completed);
            var failed = monthChanges.Count(c => c.Status == ChangeRequestStatus.Failed);
            var rolledBack = monthChanges.Count(c => c.Status == ChangeRequestStatus.RolledBack);

            var finalizedChanges = completed + failed + rolledBack;
            var successRate = finalizedChanges > 0 
                ? (decimal)completed / finalizedChanges * 100 
                : 0;

            // Calculate average completion time for the month
            var completedChanges = monthChanges
                .Where(c => c.Status == ChangeRequestStatus.Completed && 
                           c.ActualStartDate.HasValue && 
                           c.ActualEndDate.HasValue)
                .ToList();

            var avgCompletionTime = completedChanges.Any()
                ? completedChanges.Average(c => (c.ActualEndDate!.Value - c.ActualStartDate!.Value).TotalHours)
                : 0;

            monthlyData.Add(new MonthlyTrendData
            {
                Year = year,
                Month = month,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
                TotalChanges = monthChanges.Count,
                Completed = completed,
                Failed = failed,
                RolledBack = rolledBack,
                SuccessRate = Math.Round(successRate, 2),
                AverageCompletionTimeHours = Math.Round(avgCompletionTime, 2)
            });
        }

        return new MonthlyTrendsResponse
        {
            Months = monthlyData
        };
    }
}
