using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Apex.API.UseCases.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that automatically validates commands before they reach handlers
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var typeName = typeof(TRequest).Name;

        _logger.LogInformation("Validating command {CommandType}", typeName);

        // If no validators, continue to handler
        if (!_validators.Any())
        {
            _logger.LogInformation("No validators found for {CommandType}", typeName);
            return await next();
        }

        _logger.LogInformation("Found {ValidatorCount} validator(s) for {CommandType}",
            _validators.Count(), typeName);

        // Run all validators
        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Collect all validation failures
        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        // If validation failed, throw exception (FastEndpoints will handle it)
        if (failures.Any())
        {
            _logger.LogWarning(
                "Validation failed for {CommandType}. {FailureCount} error(s): {Errors}",
                typeName,
                failures.Count,
                string.Join("; ", failures.Select(f => $"{f.PropertyName}: {f.ErrorMessage}")));

            throw new ValidationException(failures);
        }

        _logger.LogInformation("Validation passed for {CommandType}", typeName);

        // Validation passed, continue to handler
        return await next();
    }
}