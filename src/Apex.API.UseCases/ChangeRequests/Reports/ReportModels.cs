namespace Apex.API.UseCases.ChangeRequests.Reports;

/// <summary>
/// Overall change management metrics
/// </summary>
public class ChangeMetricsResponse
{
    public int TotalChanges { get; set; }
    public int CompletedChanges { get; set; }
    public int FailedChanges { get; set; }
    public int RolledBackChanges { get; set; }
    public int InProgressChanges { get; set; }
    public int ScheduledChanges { get; set; }
    public int PendingApprovalChanges { get; set; }
    
    public decimal SuccessRate { get; set; }
    public decimal RollbackRate { get; set; }
    public decimal ApprovalRate { get; set; }
    
    public double AverageCompletionTimeHours { get; set; }
    public double AverageApprovalTimeHours { get; set; }
    
    public ChangesByTypeBreakdown ByType { get; set; } = new();
    public ChangesByRiskBreakdown ByRisk { get; set; } = new();
    public ChangesByPriorityBreakdown ByPriority { get; set; } = new();
}

public class ChangesByTypeBreakdown
{
    public int Standard { get; set; }
    public int Normal { get; set; }
    public int Emergency { get; set; }
}

public class ChangesByRiskBreakdown
{
    public int Low { get; set; }
    public int Medium { get; set; }
    public int High { get; set; }
    public int Critical { get; set; }
}

public class ChangesByPriorityBreakdown
{
    public int Low { get; set; }
    public int Medium { get; set; }
    public int High { get; set; }
    public int Critical { get; set; }
}

/// <summary>
/// Success rate analysis
/// </summary>
public class SuccessRateResponse
{
    public int TotalChanges { get; set; }
    public int SuccessfulChanges { get; set; }
    public int FailedChanges { get; set; }
    public int RolledBackChanges { get; set; }
    
    public decimal SuccessPercentage { get; set; }
    public decimal FailurePercentage { get; set; }
    public decimal RollbackPercentage { get; set; }
    
    public SuccessRateByType ByType { get; set; } = new();
}

public class SuccessRateByType
{
    public TypeSuccessRate Standard { get; set; } = new();
    public TypeSuccessRate Normal { get; set; } = new();
    public TypeSuccessRate Emergency { get; set; } = new();
}

public class TypeSuccessRate
{
    public int Total { get; set; }
    public int Successful { get; set; }
    public int Failed { get; set; }
    public decimal SuccessPercentage { get; set; }
}

/// <summary>
/// Approval time analysis
/// </summary>
public class ApprovalTimeResponse
{
    public double AverageApprovalTimeHours { get; set; }
    public double MedianApprovalTimeHours { get; set; }
    public double MinApprovalTimeHours { get; set; }
    public double MaxApprovalTimeHours { get; set; }
    
    public int TotalApproved { get; set; }
    public int TotalDenied { get; set; }
    public int PendingReview { get; set; }
    
    public List<ApprovalTimeBucket> TimeBuckets { get; set; } = new();
}

public class ApprovalTimeBucket
{
    public string Range { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

/// <summary>
/// Risk distribution analysis
/// </summary>
public class RiskDistributionResponse
{
    public int TotalChanges { get; set; }
    
    public RiskLevelStats Low { get; set; } = new();
    public RiskLevelStats Medium { get; set; } = new();
    public RiskLevelStats High { get; set; } = new();
    public RiskLevelStats Critical { get; set; } = new();
}

public class RiskLevelStats
{
    public int Count { get; set; }
    public decimal Percentage { get; set; }
    public int Successful { get; set; }
    public int Failed { get; set; }
    public decimal SuccessRate { get; set; }
}

/// <summary>
/// Monthly trend data
/// </summary>
public class MonthlyTrendsResponse
{
    public List<MonthlyTrendData> Months { get; set; } = new();
}

public class MonthlyTrendData
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    
    public int TotalChanges { get; set; }
    public int Completed { get; set; }
    public int Failed { get; set; }
    public int RolledBack { get; set; }
    
    public decimal SuccessRate { get; set; }
    public double AverageCompletionTimeHours { get; set; }
}

/// <summary>
/// Top affected systems
/// </summary>
public class TopAffectedSystemsResponse
{
    public List<AffectedSystemStats> Systems { get; set; } = new();
}

public class AffectedSystemStats
{
    public string SystemName { get; set; } = string.Empty;
    public int ChangeCount { get; set; }
    public int SuccessfulChanges { get; set; }
    public int FailedChanges { get; set; }
    public decimal SuccessRate { get; set; }
    public DateTime LastChangeDate { get; set; }
}
