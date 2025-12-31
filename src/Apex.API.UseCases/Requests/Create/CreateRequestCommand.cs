using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Requests.Create;

/// <summary>
/// Command to create a new request
/// </summary>
public record CreateRequestCommand(
    string Title,
    string Description,
    string? Priority = null,
    DateTime? DueDate = null
) : IRequest<Result<RequestId>>;
