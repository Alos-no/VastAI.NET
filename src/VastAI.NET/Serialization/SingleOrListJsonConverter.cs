namespace VastAI.NET.Serialization;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>List type used for Vast properties that may arrive as either one JSON object or an array.</summary>
/// <typeparam name="T">DTO type contained by the Vast response property.</typeparam>
internal sealed class SingleOrList<T> : List<T>;

/// <summary>Reads a Vast response property that can be encoded as either a single object or an array of objects.</summary>
/// <typeparam name="T">DTO type contained by the Vast response property.</typeparam>
internal sealed class SingleOrListJsonConverter<T> : JsonConverter<SingleOrList<T>>
{
  /// <summary>Reads one object into a one-item list, or reads a normal JSON array into the same list shape.</summary>
  public override SingleOrList<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    if (reader.TokenType == JsonTokenType.StartArray)
      return [.. JsonSerializer.Deserialize<List<T>>(ref reader, options) ?? []];

    if (reader.TokenType == JsonTokenType.StartObject)
    {
      var item = JsonSerializer.Deserialize<T>(ref reader, options);
      return item is null ? [] : [item];
    }

    return [];
  }

  /// <summary>Writes the normalized list form when tests serialize DTO envelopes.</summary>
  public override void Write(Utf8JsonWriter writer, SingleOrList<T> value, JsonSerializerOptions options) =>
    JsonSerializer.Serialize(writer, value.ToList(), options);
}
