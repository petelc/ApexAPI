using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.ChangeRequests.UpdateNotes;

/// <summary>
/// Command to update a ChangeRequest (only allowed in Draft status)
/// </summary>
public record UpdateNotesChangeRequestCommand(
    ChangeRequestId ChangeRequestId,
    string Notes
) : IRequest<Result>;