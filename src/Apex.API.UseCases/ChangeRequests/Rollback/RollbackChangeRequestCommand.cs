using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.ChangeRequests.Rollback;

/// <summary>
/// Command to Deny a ChangeRequest (InReview/Pending → Denied)
/// </summary>
public record RollbackChangeRequestCommand(
    ChangeRequestId ChangeRequestId,
    string Reason
) : IRequest<Result>;
