namespace TodoManagement.API.Application.Common.Validations;

/// <summary>
/// Base FluentValidation validator for commands/DTOs with reusable rules
/// and automatic access to any IReadOnlyRepository<TEntity> for DB existence checks.
/// </summary>
/// <typeparam name="T">Type to be validated (command/DTO).</typeparam>
public abstract class BaseValidator<T> : AbstractValidator<T>
{
    #region Fields

    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<Type, object> _repositoryCache = new();

    #endregion

    #region Constructor

    /// <summary>
    /// Creates a new instance of BaseValidator.
    /// </summary>
    /// <param name="serviceProvider">Scoped service provider for resolving repositories.</param>
    protected BaseValidator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    #endregion

    #region Repository Access

    /// <summary>
    /// Gets a read-only repository for the given entity type (caches instances per validator).
    /// </summary>
    protected IValidationOnlyRepository<TEntity> GetRepository<TEntity>() where TEntity : Entity
    {
        return (IValidationOnlyRepository<TEntity>)_repositoryCache.GetOrAdd(
            typeof(TEntity),
            t => _serviceProvider.GetRequiredService<IValidationOnlyRepository<TEntity>>()
        );
    }

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
    /// Validates a GUID property as a (possibly required) foreign key, and checks its existence in the database.
    /// </summary>
    protected void ValidateGuid<TEntity>(
        Expression<Func<T, Guid?>> propertyExpression,
        bool isRequired = false)
        where TEntity : Entity
    {
        var propertyName = GetPropertyName(propertyExpression);

        if (isRequired)
        {
            RuleFor(propertyExpression)
                .Must(id => id != Guid.Empty)
                .WithMessage($"{propertyName} is required.");
        }

        RuleFor(propertyExpression)
            .MustAsync(async (id, cancellation) =>
            {
                if (!id.HasValue || id == Guid.Empty) return !isRequired;
                var repo = GetRepository<TEntity>();
                return await repo.ExistsAsync(id.Value, cancellation).ConfigureAwait(false);
            })
            .When(x => propertyExpression.Compile().Invoke(x) != Guid.Empty)
            .WithMessage($"{propertyName} is invalid.");
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
                    .When(x => propertyExpression.Compile().Invoke(x) != null && !string.IsNullOrWhiteSpace(propertyExpression.Compile().Invoke(x)));
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
                        var stringValue = value.Value.ToString();
                        if (string.IsNullOrEmpty(stringValue)) return true;
                        var parts = stringValue.Split('.');
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

    /// <summary>
    /// Validates that a property value is unique in the database for the specified entity type.
    /// Generic method that works with any property type (string, int, Guid, etc.).
    /// </summary>
    /// <typeparam name="TEntity">The entity type to check uniqueness against.</typeparam>
    /// <typeparam name="TProperty">The type of the property being validated.</typeparam>
    /// <param name="propertyExpression">Expression to get the property value from the command/DTO.</param>
    /// <param name="entityFilterBuilder">Function that builds the filter expression. Takes the property value and returns an expression to compare entity properties (e.g., name => entity => entity.Name == name).</param>
    /// <remarks>
    /// The entityFilterBuilder parameter is necessary because Entity Framework needs to translate the comparison expression to SQL.
    /// We need to capture the command/DTO value and build a proper expression lambda that EF can use.
    /// This differs from ValidateGuid which validates existence (checking if a Guid FK exists), while this validates uniqueness (checking if a value is already taken).
    /// 
    /// Why ValidateGuid doesn't need entityFilterBuilder:
    /// - ValidateGuid validates foreign key existence (entity.Id == guidValue), where the property to compare is always "Id"
    /// - ValidateUniqueness can compare any property (entity.Name == name, entity.Email == email, etc.), so we need to specify which property
    /// - In ValidateGuid, we already know we're comparing against the Id property, so we can build the expression directly
    /// 
    /// Example usage: ValidateUniqueness&lt;TodoList, string&gt;(cmd => cmd.Name, name => entity => entity.Name == name)
    /// </remarks>
    protected void ValidateUniqueness<TEntity, TProperty>(
        Expression<Func<T, TProperty?>> propertyExpression,
        Func<TProperty, Expression<Func<TEntity, bool>>> entityFilterBuilder)
        where TEntity : Entity
        where TProperty : notnull
    {
        var propertyName = GetPropertyName(propertyExpression);

        RuleFor(propertyExpression)
            .MustAsync(async (value, cancellation) =>
            {
                if (value == null) return true; // Skip validation if null (should be handled by required validation)
                if (value is string strValue && string.IsNullOrWhiteSpace(strValue)) return true; // Skip validation if empty string

                var repo = GetRepository<TEntity>();
                // Build the filter expression using the provided builder function
                // This captures the value and creates: entity => entity.Property == value
                var filter = entityFilterBuilder(value);
                
                // Check if any entity exists with the same value (invert result: we want it to NOT exist)
                return !await repo.ExistsAsync(filter, cancellation);
            })
            .When(x =>
            {
                var value = propertyExpression.Compile().Invoke(x);
                if (value == null) return false;
                if (value is string strValue && string.IsNullOrWhiteSpace(strValue)) return false;
                return true;
            })
            .WithMessage($"{propertyName} already exists.");
    }

    /// <summary>
    /// Validates that an entity exists in the database using a custom filter expression.
    /// This method is useful for validating existence using composite keys or multiple properties.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to check existence against.</typeparam>
    /// <typeparam name="TProperty">The type of the property being validated (can be a complex type like a DTO).</typeparam>
    /// <param name="propertyExpression">Expression to get the property value from the command/DTO.</param>
    /// <param name="entityFilterBuilder">Function that builds the filter expression. Takes the property value and returns an expression to compare entity properties (e.g., dto => entity => entity.TodoListId == dto.TodoListId && entity.ItemId == dto.ItemId).</param>
    /// <param name="errorMessage">Custom error message to display if the entity does not exist.</param>
    /// <remarks>
    /// The entityFilterBuilder parameter is necessary because Entity Framework needs to translate the comparison expression to SQL.
    /// We need to capture the command/DTO value and build a proper expression lambda that EF can use.
    /// 
    /// Example usage: ValidateExists&lt;TodoItem, UpdateTodoItemDto&gt;(
    ///     cmd => cmd.TodoItem,
    ///     dto => entity => entity.TodoListId == dto.TodoListId && entity.ItemId == dto.ItemId,
    ///     "TodoItem does not exist.")
    /// </remarks>
    protected void ValidateExists<TEntity, TProperty>(
        Expression<Func<T, TProperty>> propertyExpression,
        Func<TProperty, Expression<Func<TEntity, bool>>> entityFilterBuilder,
        string errorMessage = "Entity does not exist.")
        where TEntity : Entity
    {
        RuleFor(propertyExpression)
            .MustAsync(async (value, cancellation) =>
            {
                if (value == null) return false;
                
                var repo = GetRepository<TEntity>();
                // Build the filter expression using the provided builder function
                // This captures the value and creates: entity => entity.Property1 == value.Property1 && entity.Property2 == value.Property2
                var filter = entityFilterBuilder(value);
                
                // Check if entity exists with the specified filter
                return await repo.ExistsAsync(filter, cancellation);
            })
            .WithMessage(errorMessage);
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