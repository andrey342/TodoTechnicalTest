namespace TodoManagement.Domain.SeedWork;

/// <summary>
/// Represents a base class for value objects in Domain-Driven Design.
/// Value objects are compared based on their property values rather than by reference.
/// </summary>
public abstract class ValueObject
{
    /// <summary>
    /// Returns the atomic values that are used for equality comparison.
    /// Derived classes must override this method to provide the relevant components.
    /// </summary>
    /// <returns>An enumerable of objects representing the equality components.</returns>
    protected abstract IEnumerable<object> GetEqualityComponents();

    /// <summary>
    /// Determines whether the specified object is equal to the current value object.
    /// Equality is based on the values returned by <see cref="GetEqualityComponents"/>.
    /// </summary>
    /// <param name="obj">The object to compare with the current value object.</param>
    /// <returns>True if the objects are equal; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
            return false;

        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <summary>
    /// Returns a hash code for the value object based on its equality components.
    /// </summary>
    /// <returns>A hash code for the current value object.</returns>
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Aggregate(1, (current, obj) => HashCode.Combine(current, obj));
    }
}