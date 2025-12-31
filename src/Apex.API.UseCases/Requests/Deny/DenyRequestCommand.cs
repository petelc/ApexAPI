using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Requests.Deny;

/// <summary>
/// Command to Deny a request (InReview/Pending → Denied)
/// </summary>
public record DenyRequestCommand(
    RequestId RequestId,
    string Reason
) : IRequest<Result>;
