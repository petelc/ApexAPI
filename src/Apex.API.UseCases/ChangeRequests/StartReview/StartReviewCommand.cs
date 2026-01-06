using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.ChangeRequests.StartReview;

/// <summary>
/// Command to submit a ChangeRequest for review (Draft → Pending)
/// </summary>
public record StartReviewChangeRequestCommand(ChangeRequestId ChangeRequestId) : IRequest<Result>;