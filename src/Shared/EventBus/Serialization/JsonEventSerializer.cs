namespace EventBus.Serialization;

/// <summary>
/// Implementation of IEventSerializer using JSON and optional GZip compression
/// </summary>
public sealed class JsonEventSerializer : IEventSerializer
{
    // GZip magic numbers for format detection
    private const int GzipMagic1 = 0x1F, GzipMagic2 = 0x8B;

    // JSON serialization options: camel case naming, no indentation
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes an event to JSON format and compresses the result if the payload size exceeds the defined threshold.
    /// </summary>
    /// <typeparam name="T">Type of the event to serialize.</typeparam>
    /// <param name="evt">Event instance to serialize.</param>
    /// <returns>Byte array representing the serialized (and possibly compressed) event.</returns>
    public byte[] Serialize<T>(T evt)
    {
        var json = JsonSerializer.Serialize(evt, Options);
        var bytes = Encoding.UTF8.GetBytes(json);

        // Compress if the payload is larger than 150,000 bytes
        return bytes.Length < 150_000
            ? bytes // uncompressed
            : Compress(bytes);
    }

    /// <summary>
    /// Deserializes a byte array to an event instance, automatically decompressing if the data is GZip-compressed.
    /// </summary>
    /// <typeparam name="T">Type of the event to deserialize.</typeparam>
    /// <param name="data">Byte array containing the serialized event data.</param>
    /// <returns>Deserialized event instance.</returns>
    public T Deserialize<T>(byte[] data)
    {
        Stream stream;
        // Check for GZip header to determine if decompression is needed
        if (data[0] == GzipMagic1 && data[1] == GzipMagic2)
        {
            stream = new GZipStream(new MemoryStream(data), CompressionMode.Decompress);
        }
        else
        {
            stream = new MemoryStream(data);
        }

        return JsonSerializer.Deserialize<T>(stream, Options)!;
    }

    /// <summary>
    /// Compresses a byte array using GZip compression.
    /// </summary>
    /// <param name="bytes">Byte array to compress.</param>
    /// <returns>Compressed byte array.</returns>
    private static byte[] Compress(byte[] bytes)
    {
        using var outStream = new MemoryStream();
        using (var gzip = new GZipStream(outStream, CompressionLevel.Fastest, leaveOpen: true))
            gzip.Write(bytes, 0, bytes.Length);
        return outStream.ToArray();
    }
}