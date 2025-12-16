namespace TodoManagement.Domain.SeedWork;

/// <summary>
/// Represents the base class for all domain entities in the system.
/// Provides identity, domain event management, and property copying utilities.
/// </summary>
public abstract class Entity
{
    #region Identity

    /// <summary>
    /// Unique identifier for the entity.
    /// </summary>
    public Guid Id { get; protected set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="Entity"/> class.
    /// Generates a new Guid if the Id is not already set.
    /// </summary>
    protected Entity()
    {
        // Generate a new Guid if the Id is empty.
        if (Id == Guid.Empty)
            Id = Guid.NewGuid();
    }

    #endregion

    #region Domain Events

    /// <summary>
    /// Internal list of domain events associated with this entity.
    /// </summary>
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Read-only collection of domain events for external access.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event to the entity's event collection.
    /// </summary>
    /// <param name="domainEvent">The domain event to add.</param>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Removes a domain event from the entity's event collection.
    /// </summary>
    /// <param name="domainEvent">The domain event to remove.</param>
    protected void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// Clears all domain events from the entity.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    #endregion

    #region Equality

    /// <summary>
    /// Determines whether the specified object is equal to the current entity.
    /// </summary>
    /// <param name="obj">The object to compare with the current entity.</param>
    /// <returns>True if the objects are equal; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
            return false;

        var other = (Entity)obj;
        return Id == other.Id;
    }

    /// <summary>
    /// Returns a hash code for the entity.
    /// </summary>
    /// <returns>A hash code for the current entity.</returns>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    #endregion

    #region Property Copying

    // -------------------------------
    // FAST method for < 10 properties
    // -------------------------------
    /// <summary>
    /// Copies properties from another entity of the same type.
    /// Maintains value object logic and avoids copying collections.
    /// Recommended for entities with a small number of properties.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="source">The source entity to copy from.</param>
    public void CopyPropertiesTo<T>(T source) where T : Entity
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(prop =>
                prop.Name != nameof(Id) &&
                prop.CanRead && prop.CanWrite &&
                !typeof(IEnumerable<object>).IsAssignableFrom(prop.PropertyType) && // Excludes IEnumerable<T>
                (prop.PropertyType.IsPrimitive ||
                 prop.PropertyType.IsValueType ||
                 prop.PropertyType == typeof(string) ||
                 typeof(ValueObject).IsAssignableFrom(prop.PropertyType))); // Excludes List<T>, HashSet<T>, etc.

        foreach (var prop in properties)
        {
            var sourceValue = prop.GetValue(source);
            // If the property is a Value Object, instantiate a new instance using its constructor.
            if (typeof(ValueObject).IsAssignableFrom(prop.PropertyType))
            {
                if (sourceValue != null)
                {
                    var constructor = prop.PropertyType.GetConstructor(new[] { sourceValue.GetType() });
                    if (constructor != null)
                    {
                        var newEntityInstance = constructor.Invoke(new[] { sourceValue });
                        prop.SetValue(this, newEntityInstance);
                    }
                }
            }
            // If the property is an Entity, use its constructor if available.
            else if (typeof(Entity).IsAssignableFrom(prop.PropertyType))
            {
                if (sourceValue != null)
                {
                    var constructor = prop.PropertyType.GetConstructor(new[] { sourceValue.GetType() });
                    if (constructor != null)
                    {
                        var newEntityInstance = constructor.Invoke(new[] { sourceValue });
                        prop.SetValue(this, newEntityInstance);
                    }
                }
            }
            // If the property is a primitive type, copy the value directly.
            else
            {
                prop.SetValue(this, sourceValue);
            }
        }
    }

    // -------------------------------
    // VERY FAST method for >10 properties
    // -------------------------------
    /// <summary>
    /// Efficiently copies properties from another entity of the same type.
    /// Utilizes compiled expressions and parallelization for improved performance on entities with many properties.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="source">The source entity to copy from.</param>
    public void CopyPropertiesFast<T>(T source) where T : Entity
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(prop =>
                prop.Name != nameof(Id) &&
                prop.CanRead && prop.CanWrite &&
                !typeof(IEnumerable<object>).IsAssignableFrom(prop.PropertyType) && // Excludes IEnumerable<T>
                (prop.PropertyType.IsPrimitive ||
                 prop.PropertyType.IsValueType ||
                 prop.PropertyType == typeof(string) ||
                 typeof(ValueObject).IsAssignableFrom(prop.PropertyType)))// Excludes List<T>, HashSet<T>, etc.
            .ToArray(); // Convert to array for parallel optimization

        // Cache of dynamic property setters to minimize reflection overhead.
        var setters = new ConcurrentDictionary<PropertyInfo, Action<T, object>>();

        Parallel.ForEach(properties, prop =>
        {
            var setter = setters.GetOrAdd(prop, CreateSetter<T>(prop)); // Retrieve or create a compiled setter.
            var sourceValue = prop.GetValue(source);
            // If the property is a Value Object, instantiate a new instance.
            if (typeof(ValueObject).IsAssignableFrom(prop.PropertyType))
            {
                if (sourceValue != null)
                {
                    var constructor = prop.PropertyType.GetConstructor(new[] { sourceValue.GetType() });
                    if (constructor != null)
                    {
                        var newValueObject = constructor.Invoke(new[] { sourceValue });
                        setter(this as T, newValueObject);
                    }
                }
            }
            // If the property is an Entity, use its constructor if available.
            else if (typeof(Entity).IsAssignableFrom(prop.PropertyType))
            {
                if (sourceValue != null)
                {
                    var constructor = prop.PropertyType.GetConstructor(new[] { sourceValue.GetType() });
                    if (constructor != null)
                    {
                        var newEntityInstance = constructor.Invoke(new[] { sourceValue });
                        setter(this as T, newEntityInstance);
                    }
                }
            }
            // If the property is a primitive type, copy the value directly.
            else
            {
                setter(this as T, sourceValue);
            }
        });
    }

    /// <summary>
    /// Generates a compiled lambda expression to efficiently assign values to a property.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="prop">The property to create a setter for.</param>
    /// <returns>A compiled action that sets the property value.</returns>
    private static Action<T, object> CreateSetter<T>(PropertyInfo prop) where T : Entity
    {
        var targetExp = Expression.Parameter(typeof(T), "target");
        var valueExp = Expression.Parameter(typeof(object), "value");

        var convertedValueExp = Expression.Convert(valueExp, prop.PropertyType);
        var propertyExp = Expression.Property(targetExp, prop);
        var assignExp = Expression.Assign(propertyExp, convertedValueExp);

        return Expression.Lambda<Action<T, object>>(assignExp, targetExp, valueExp).Compile();
    }

    #endregion
}