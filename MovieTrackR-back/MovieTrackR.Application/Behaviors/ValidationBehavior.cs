using FluentValidation;
using MediatR;

namespace MovieTrackR.Application.behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (validators.Any())
        {
            ValidationContext<TRequest> validationContext = new ValidationContext<TRequest>(request);
            List<FluentValidation.Results.ValidationFailure>? errors = (await Task.WhenAll(validators.Select(v => v.ValidateAsync(validationContext, cancellationToken))))
                         .SelectMany(r => r.Errors)
                         .Where(e => e is not null)
                         .ToList();

            if (errors.Count != 0)
                throw new ValidationException(errors);
        }

        return await next();
    }
}