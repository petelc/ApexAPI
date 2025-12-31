using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Requests.Update;

/// <summary>
/// Command to update a request (only allowed in Draft status)
/// </summary>
public record UpdateRequestCommand(
    RequestId RequestId,
    string Title,
    string Description,
    string? Priority = null,
    DateTime? DueDate = null
) : IRequest<Result>;