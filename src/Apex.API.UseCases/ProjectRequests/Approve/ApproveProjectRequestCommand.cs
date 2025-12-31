using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.ProjectRequests.Approve;

/// <summary>
/// Command to approve a ProjectRequest (InReview/Pending → Approved)
/// </summary>
public record ApproveProjectRequestCommand(
    ProjectRequestId ProjectRequestId,
    string? Notes = null
) : IRequest<Result>;
