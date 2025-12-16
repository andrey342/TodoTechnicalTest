namespace TodoManagement.API.Application.Behaviors;

/// <summary>
/// Pipeline behavior for validating requests using FluentValidation.
/// Executes all validators in parallel and throws a ValidationException if any errors are found.
/// Applies to any Mediator request (commands, queries, etc.).
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="validators">A collection of validators for the request type.</param>
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    /// <summary>
    /// Handles the validation logic before passing the request to the next handler in the pipeline.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The response from the next handler in the pipeline.</returns>
    /// <exception cref="FluentValidation.ValidationException">Thrown if validation failures are found.</exception>
    public async ValueTask<TResponse> Handle(
        TRequest request,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            // Execute all validators in parallel and collect unique errors
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .GroupBy(f => new { f.PropertyName, f.ErrorMessage }) // Avoid duplicates
                .Select(g => g.First()) // Take only the first from each group
                .ToList();

            if (failures.Any())
            {
                throw new FluentValidation.ValidationException(failures);
            }
        }

        return await next(request, cancellationToken);
    }
}