using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Tasks.Complete;

/// <summary>
/// Command to complete a task with optional resolution notes
/// </summary>
public record CompleteTaskCommand(
    TaskId TaskId,
    string? ResolutionNotes = null
) : IRequest<Result>;
