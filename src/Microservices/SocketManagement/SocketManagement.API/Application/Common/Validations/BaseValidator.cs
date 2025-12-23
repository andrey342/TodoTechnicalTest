namespace SocketManagement.API.Application.Common.Validations;

/// <summary>
/// Base FluentValidation validator for commands/DTOs with reusable rules
/// </summary>
/// <typeparam name="T">Type to be validated (command/DTO).</typeparam>
public abstract class BaseValidator<T> : AbstractValidator<T>
{

    #region Constructor

    /// <summary>
    /// Creates a new instance of BaseValidator.
    /// </summary>
    protected BaseValidator() {}

    #endregion

    #region Validation Methods

    /// <summary>
    /// Adds a required rule for the specified property. 
    /// This rule enforces that the property is not empty (works for strings, Guids, collections, and nullable structs).
    /// </summary>
    protected void Require<TProp>(Expression<Func<T, TProp>> propertyExpression)
    {
        var propertyName = GetPropertyName(propertyExpression);

        RuleFor(propertyExpression)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()               // Funciona para string, Guid, colecciones, nullable structs…
            .WithMessage($"{propertyName} is required.");
    }

    /// <summary>
    /// Validates a string property as required or optional, with maximum length.
    /// </summary>
    protected void ValidateString(
        Expression<Func<T, string?>> propertyExpression,
        int maxLength = 100,
        bool isRequired = false)
    {
        var propertyName = GetPropertyName(propertyExpression);

        RuleFor(propertyExpression)
            .NotEmpty().WithMessage($"{propertyName} is required.")
            .When(_ => isRequired)
            .DependentRules(() =>
            {
                RuleFor(propertyExpression)
                    .MaximumLength(maxLength)
                    .WithMessage($"{propertyName} must not exceed {maxLength} characters.")
                    .When(x => !string.IsNullOrWhiteSpace(propertyExpression.Compile().Invoke(x)));
            });
    }

    /// <summary>
    /// Validates a nullable boolean as required if specified.
    /// </summary>
    protected void ValidateBoolean(
        Expression<Func<T, bool?>> propertyExpression,
        bool isRequired = false)
    {
        var propertyName = GetPropertyName(propertyExpression);

        RuleFor(propertyExpression)
            .NotNull().WithMessage($"{propertyName} is required.")
            .When(_ => isRequired);
    }

    /// <summary>
    /// Validates a nullable decimal property for required, precision and scale.
    /// </summary>
    protected void ValidateDecimal(
        Expression<Func<T, decimal?>> propertyExpression,
        bool isRequired = false,
        int precision = int.MaxValue,
        int scale = int.MaxValue)
    {
        var propertyName = GetPropertyName(propertyExpression);

        RuleFor(propertyExpression)
            .NotNull().WithMessage($"{propertyName} is required.")
            .When(_ => isRequired)
            .DependentRules(() =>
            {
                RuleFor(propertyExpression)
                    .Must(value =>
                    {
                        if (!value.HasValue) return true;
                        var parts = value.ToString().Split('.');
                        var integerDigits = parts[0].Length;
                        var fractionalDigits = parts.Length > 1 ? parts[1].Length : 0;
                        return integerDigits + fractionalDigits <= precision && fractionalDigits <= scale;
                    })
                    .WithMessage($"{propertyName} must have up to {precision - scale} digits before and {scale} digits after the decimal point.")
                    .When(x => propertyExpression.Compile().Invoke(x).HasValue);
            });
    }

    /// <summary>
    /// Validates a nullable DateTime property for required, and that it is in the past/future.
    /// </summary>
    protected void ValidateDate(
        Expression<Func<T, DateTime?>> propertyExpression,
        bool isRequired = false,
        bool isFutureDate = false)
    {
        var propertyName = GetPropertyName(propertyExpression);

        if (isRequired)
        {
            RuleFor(propertyExpression)
                .Must(date => date.HasValue && date.Value != DateTime.MinValue)
                .WithMessage($"{propertyName} is required.");
        }

        RuleFor(propertyExpression)
            .Must(date => !date.HasValue || (isFutureDate ? date >= DateTime.UtcNow : date <= DateTime.UtcNow))
            .WithMessage(isFutureDate
                ? $"{propertyName} must be in the future."
                : $"{propertyName} must be in the past.")
            .When(x => propertyExpression.Compile().Invoke(x).HasValue && propertyExpression.Compile().Invoke(x) != DateTime.MinValue);
    }

    /// <summary>
    /// Validates an email property for required and correct format.
    /// </summary>
    protected void ValidateEmail(
        Expression<Func<T, string?>> propertyExpression,
        bool isRequired = false)
    {
        var propertyName = GetPropertyName(propertyExpression);

        RuleFor(propertyExpression)
            .NotEmpty().WithMessage($"{propertyName} is required.")
            .When(_ => isRequired)
            .DependentRules(() =>
            {
                RuleFor(propertyExpression)
                    .EmailAddress().WithMessage("Invalid email format.")
                    .When(x => !string.IsNullOrWhiteSpace(propertyExpression.Compile().Invoke(x)));
            });
    }

    /// <summary>
    /// Validates a phone number property (E.164 international format), required or optional.
    /// </summary>
    protected void ValidatePhoneNumber(
        Expression<Func<T, string?>> propertyExpression,
        bool isRequired = false)
    {
        var propertyName = GetPropertyName(propertyExpression);

        RuleFor(propertyExpression)
            .NotEmpty().WithMessage($"{propertyName} is required.")
            .When(_ => isRequired)
            .DependentRules(() =>
            {
                RuleFor(propertyExpression)
                    .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage($"{propertyName} format is invalid.")
                    .When(x => !string.IsNullOrWhiteSpace(propertyExpression.Compile().Invoke(x)));
            });
    }

    /// <summary>
    /// Validates a nullable integer property to be positive, required or optional.
    /// </summary>
    protected void ValidatePositiveNumber(
        Expression<Func<T, int?>> propertyExpression,
        bool isRequired = false)
    {
        var propertyName = GetPropertyName(propertyExpression);

        if (isRequired)
        {
            RuleFor(propertyExpression)
                .NotEmpty()
                .WithMessage($"{propertyName} is required.");
        }

        RuleFor(propertyExpression)
            .Must(number => !number.HasValue || number > 0)
            .WithMessage($"{propertyName} must be a positive number.")
            .When(x => propertyExpression.Compile().Invoke(x).HasValue);
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Gets a property name from a lambda expression.
    /// </summary>
    private static string GetPropertyName<TProperty>(Expression<Func<T, TProperty>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
            return memberExpression.Member.Name;
        if (expression.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression operand)
            return operand.Member.Name;
        throw new ArgumentException($"Invalid property expression: {expression}");
    }

    #endregion
}