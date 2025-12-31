using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.ProjectRequests.Deny;

/// <summary>
/// Command to Deny a ProjectRequest (InReview/Pending → Denied)
/// </summary>
public record DenyProjectRequestCommand(
    ProjectRequestId ProjectRequestId,
    string Reason
) : IRequest<Result>;
