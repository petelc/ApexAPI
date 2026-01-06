using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.ChangeRequests.Approve;

/// <summary>
/// Command to approve a ChangeRequest (InReview/Pending → Approved)
/// </summary>
public record ApproveChangeRequestCommand(
    ChangeRequestId ChangeRequestId,
    string? Notes = null
) : IRequest<Result>;
