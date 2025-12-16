namespace EventBus.Abstractions;

/// <summary>
/// Interface for event serialization and deserialization
/// </summary>
public interface IEventSerializer
{
    // Serializes an event to a byte array
    byte[] Serialize<T>(T @event);

    // Deserializes a byte array to an event
    T Deserialize<T>(byte[] data);
}