namespace Apex.API.Infrastructure.Email.Templates;

/// <summary>
/// Base model for all email templates
/// </summary>
public abstract class EmailTemplateModel
{
    public string BaseUrl { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
}

/// <summary>
/// Email when change request is submitted for CAB review
/// </summary>
public class ChangeRequestSubmittedEmail : EmailTemplateModel
{
    public string ChangeRequestId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ChangeType { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = string.Empty;
    public string SubmittedBy { get; set; } = string.Empty;
    public DateTime SubmittedDate { get; set; }
    public string ViewUrl => $"{BaseUrl}/change-requests/{ChangeRequestId}";
    public string ReviewUrl => $"{BaseUrl}/cab/review/{ChangeRequestId}";
}

/// <summary>
/// Email when change request is approved
/// </summary>
public class ChangeRequestApprovedEmail : EmailTemplateModel
{
    public string ChangeRequestId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ApprovedBy { get; set; } = string.Empty;
    public DateTime ApprovedDate { get; set; }
    public string? ApprovalNotes { get; set; }
    public string ViewUrl => $"{BaseUrl}/change-requests/{ChangeRequestId}";
}

/// <summary>
/// Email when change request is denied
/// </summary>
public class ChangeRequestDeniedEmail : EmailTemplateModel
{
    public string ChangeRequestId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string DeniedBy { get; set; } = string.Empty;
    public DateTime DeniedDate { get; set; }
    public string DenialReason { get; set; } = string.Empty;
    public string ViewUrl => $"{BaseUrl}/change-requests/{ChangeRequestId}";
}

/// <summary>
/// Email when change is scheduled
/// </summary>
public class ChangeScheduledEmail : EmailTemplateModel
{
    public string ChangeRequestId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime ScheduledStartDate { get; set; }
    public DateTime ScheduledEndDate { get; set; }
    public string ChangeWindow { get; set; } = string.Empty;
    public string AffectedSystems { get; set; } = string.Empty;
    public string ViewUrl => $"{BaseUrl}/change-requests/{ChangeRequestId}";
}

/// <summary>
/// Email reminder before change starts
/// </summary>
public class ChangeReminderEmail : EmailTemplateModel
{
    public string ChangeRequestId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime ScheduledStartDate { get; set; }
    public string ChangeWindow { get; set; } = string.Empty;
    public int HoursUntilStart { get; set; }
    public string AffectedSystems { get; set; } = string.Empty;
    public string RollbackPlan { get; set; } = string.Empty;
    public string ViewUrl => $"{BaseUrl}/change-requests/{ChangeRequestId}";
}

/// <summary>
/// Email when change is completed
/// </summary>
public class ChangeCompletedEmail : EmailTemplateModel
{
    public string ChangeRequestId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime CompletedDate { get; set; }
    public DateTime ActualStartDate { get; set; }
    public DateTime ActualEndDate { get; set; }
    public string? ImplementationNotes { get; set; }
    public string ViewUrl => $"{BaseUrl}/change-requests/{ChangeRequestId}";
}

/// <summary>
/// Email when change fails and is rolled back
/// </summary>
public class ChangeRolledBackEmail : EmailTemplateModel
{
    public string ChangeRequestId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime RolledBackDate { get; set; }
    public string RollbackReason { get; set; } = string.Empty;
    public string AffectedSystems { get; set; } = string.Empty;
    public string ViewUrl => $"{BaseUrl}/change-requests/{ChangeRequestId}";
    public string IncidentUrl => $"{BaseUrl}/incidents/create?changeRequestId={ChangeRequestId}";
}
