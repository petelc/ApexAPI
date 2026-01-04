using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Tasks.LogTime;

public record LogTimeCommand(
    TaskId TaskId,
    decimal Hours
) : IRequest<Result<decimal>>;