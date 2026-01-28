using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Tasks.UpdateNotes;

/// <summary>
/// Command to update task implementation notes
/// </summary>
public record UpdateImplementationNotesCommand(
    TaskId TaskId,
    string? Notes
) : IRequest<Result>;
