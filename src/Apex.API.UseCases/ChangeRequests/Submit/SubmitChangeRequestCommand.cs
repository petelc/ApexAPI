using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.ChangeRequests.Submit;

/// <summary>
/// Command to submit a ChangeRequest for review (Draft → Pending)
/// </summary>
public record SubmitChangeRequestCommand(ChangeRequestId ChangeRequestId) : IRequest<Result>;