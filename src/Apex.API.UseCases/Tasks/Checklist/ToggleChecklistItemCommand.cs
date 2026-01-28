using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Tasks.Checklist;

/// <summary>
/// Command to toggle a checklist item's completion status
/// </summary>
public record ToggleChecklistItemCommand(
    TaskChecklistItemId ItemId
) : IRequest<Result>;
