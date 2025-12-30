using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;


namespace Apex.API.UseCases.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that automatically validates commands before they reach handlers
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResult
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

        // If validation failed, return Invalid result (not throw exception)
        if (failures.Any())
        {
            _logger.LogWarning(
                "Validation failed for {CommandType}. {FailureCount} error(s): {Errors}",
                typeName,
                failures.Count,
                string.Join("; ", failures.Select(f => $"{f.PropertyName}: {f.ErrorMessage}")));

            // Convert FluentValidation errors to Ardalis.Result ValidationErrors
            var validationErrors = failures
                .Select(f => new ValidationError
                {
                    Identifier = f.PropertyName,
                    ErrorMessage = f.ErrorMessage,
                    Severity = ValidationSeverity.Error
                })
                .ToArray();

            // Create Invalid result using reflection (since we don't know the exact TResponse type)
            var resultType = typeof(TResponse);

            if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                // Result<T>
                var valueType = resultType.GetGenericArguments()[0];
                var invalidMethod = typeof(Result<>)
                    .MakeGenericType(valueType)
                    .GetMethod(nameof(Result<object>.Invalid), new[] { typeof(ValidationError[]) });

                var result = invalidMethod?.Invoke(null, new object[] { validationErrors });
                return (TResponse)result!;
            }
            else
            {
                // Result (non-generic)
                var invalidMethod = typeof(Result)
                    .GetMethod(nameof(Result.Invalid), new[] { typeof(ValidationError[]) });

                var result = invalidMethod?.Invoke(null, new object[] { validationErrors });
                return (TResponse)result!;
            }
        }

        _logger.LogInformation("Validation passed for {CommandType}", typeName);

        // Validation passed, continue to handler
        return await next();
    }
}