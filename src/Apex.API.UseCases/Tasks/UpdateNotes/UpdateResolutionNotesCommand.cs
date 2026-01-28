using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Tasks.UpdateNotes;

/// <summary>
/// Command to update task resolution notes
/// </summary>
public record UpdateResolutionNotesCommand(
    TaskId TaskId,
    string? Notes
) : IRequest<Result>;
