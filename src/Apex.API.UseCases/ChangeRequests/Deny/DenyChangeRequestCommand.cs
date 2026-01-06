using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.ChangeRequests.Deny;

/// <summary>
/// Command to Deny a ChangeRequest (InReview/Pending → Denied)
/// </summary>
public record DenyChangeRequestCommand(
    ChangeRequestId ChangeRequestId,
    string Reason
) : IRequest<Result>;
