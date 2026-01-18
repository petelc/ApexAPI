using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;
using Apex.API.UseCases.Tasks.DTOs;

namespace Apex.API.UseCases.Tasks.GetById;

public record GetTaskByIdQuery(TaskId TaskId) : IRequest<Result<TaskDto>>;