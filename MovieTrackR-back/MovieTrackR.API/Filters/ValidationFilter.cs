using FluentValidation;

namespace MovieTrackR.API.Filters;

public sealed class ValidationFilter<T> : IEndpointFilter where T : class
{
    private readonly IValidator<T> _validator;
    public ValidationFilter(IValidator<T> validator) => _validator = validator;

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
    {
        // récupère le 1er argument du handler qui est de type T
        var dto = ctx.Arguments.FirstOrDefault(a => a is T) as T;
        if (dto is not null)
        {
            var result = await _validator.ValidateAsync(dto);
            if (!result.IsValid)
                throw new FluentValidation.ValidationException(result.Errors);
        }

        return await next(ctx);
    }
}
