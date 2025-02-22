using System.IO;
using System.Text;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace Cognassist.Notification.Application;

/// <summary>
/// A custom Cosmos DB serialiser that uses the JSON.NET (Newtonsoft.Json) library for serialisation and deserialisation.
/// </summary>
public sealed class NewtonsoftCosmosSerialiser(JsonSerializerSettings jsonSerialiserSettings) : CosmosSerializer
{
    private static readonly Encoding DefaultEncoding = new UTF8Encoding(false, true);

    /// <summary>
    /// Deserialises a stream into an object of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of object to deserialize into.</typeparam>
    /// <param name="stream">A readable stream containing JSON data.</param>
    /// <returns>The deserialized object of type <typeparamref name="T"/>.</returns>
    public override T FromStream<T>(Stream stream)
    {
        if (stream == null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        // If the target type is Stream, return the stream directly without deserialization.
        if (typeof(Stream).IsAssignableFrom(typeof(T)))
        {
            return (T)(object)stream;
        }

        using (var streamReader = new StreamReader(stream, DefaultEncoding, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: false))
        using (var jsonTextReader = new JsonTextReader(streamReader))
        {
            var jsonSerializer = CreateJsonSerialiser();
            return jsonSerializer.Deserialize<T>(jsonTextReader);
        }
    }

    /// <summary>
    /// Serialises an object into a stream containing JSON data.
    /// </summary>
    /// <typeparam name="T">The type of object being serialised.</typeparam>
    /// <param name="input">The object to serialise.</param>
    /// <returns>A stream containing the JSON representation of the object.</returns>
    public override Stream ToStream<T>(T input)
    {
        if (input == null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        var memoryStream = new MemoryStream();

        using (var streamWriter = new StreamWriter(memoryStream, encoding: DefaultEncoding, bufferSize: 1024, leaveOpen: true))
        using (var jsonTextWriter = new JsonTextWriter(streamWriter))
        {
            jsonTextWriter.Formatting = Formatting.None;
            var jsonSerializer = CreateJsonSerialiser();
            jsonSerializer.Serialize(jsonTextWriter, input);
            jsonTextWriter.Flush();
            streamWriter.Flush();
        }

        memoryStream.Position = 0;
        return memoryStream;
    }

    /// <summary>
    /// Creates a new instance of <see cref="JsonSerialiser"/> with the configured settings.
    /// This avoids potential race conditions when reusing a shared serialiser instance.
    /// </summary>
    /// <returns>A new <see cref="JsonSerialiser"/> instance.</returns>
    private JsonSerializer CreateJsonSerialiser()
    {
        return JsonSerializer.Create(jsonSerialiserSettings);
    }
}