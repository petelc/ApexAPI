using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Projects.PutOnHold;

/// <summary>
/// Command to put a project on hold
/// </summary>
public record PutProjectOnHoldCommand(
    ProjectId ProjectId,
    string Reason
) : IRequest<Result>;
