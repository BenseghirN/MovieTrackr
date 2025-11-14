using FluentValidation;
using FluentValidation.Results;

namespace MovieTrackR.API.Filters;

public sealed class ValidationFilter<T> : IEndpointFilter where T : class
{
    private readonly IValidator<T> _validator;
    public ValidationFilter(IValidator<T> validator) => _validator = validator;

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        // récupère le 1er argument du handler qui est de type T
        var dto = context.Arguments.FirstOrDefault(a => a is T) as T;
        if (dto is not null)
        {
            ValidationResult result = await _validator.ValidateAsync(dto);
            if (!result.IsValid)
                throw new ValidationException(result.Errors);
        }

        return await next(context);
    }
}
