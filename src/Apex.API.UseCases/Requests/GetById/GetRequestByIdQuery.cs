using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Requests.GetById;

/// <summary>
/// Query to get a request by ID
/// </summary>
public record GetRequestByIdQuery(RequestId RequestId) : IRequest<Result<RequestDto>>;

/// <summary>
/// Request DTO for read operations
/// </summary>
public record RequestDto(
    Guid Id,
    string Title,
    string Description,
    string Status,
    string Priority,
    Guid CreatedByUserId,
    Guid? AssignedToUserId,
    Guid? ApprovedByUserId,
    DateTime CreatedDate,
    DateTime? SubmittedDate,
    DateTime? ApprovedDate,
    DateTime? CompletedDate,
    DateTime? DueDate,
    string? ApprovalNotes,
    string? DenialReason,
    bool IsOverdue,
    int? DaysUntilDue);
