using FluentValidation;
using MediatR;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : class
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationFailures = await Task.WhenAll(
            _validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        var errors = validationFailures
            .SelectMany(validationResult => validationResult.Errors)
            .Where(failure => failure != null)
            .Select(failure => new Error(failure.PropertyName, failure.ErrorMessage, ErrorType.Validation))
            .Distinct()
            .ToArray();

        if (errors.Any())
        {
            // This reflection logic dynamically creates a Failure Result of the correct type (e.g., Result or Result<T>)
            var resultType = typeof(TResponse);

            if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var genericArgument = resultType.GetGenericArguments()[0];
                var genericResultType = typeof(Result<>).MakeGenericType(genericArgument);
                var failureMethod = genericResultType.GetMethod(nameof(Result.Failure), new[] { typeof(Error[]) });
                if (failureMethod != null)
                {
                    return (failureMethod.Invoke(null, new object[] { errors }) as TResponse)!;
                }
            }
            else if (resultType == typeof(Result))
            {
                var failureMethod = typeof(Result).GetMethod(nameof(Result.Failure), new[] { typeof(Error[]) });
                if (failureMethod != null)
                {
                    return (failureMethod.Invoke(null, new object[] { errors }) as TResponse)!;
                }
            }
        }

        return await next();
    }
}
