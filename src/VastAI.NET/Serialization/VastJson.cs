namespace VastAI.NET.Serialization;

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>Centralizes JSON options and response-shape handling for Vast REST payloads.</summary>
internal static class VastJson
{
  private static readonly JsonSerializerOptions Options = new()
  {
    PropertyNamingPolicy        = JsonNamingPolicy.SnakeCaseLower,
    PropertyNameCaseInsensitive = true,
    Converters                  = { new FlexibleDoubleConverter(), new FlexibleStringConverter() }
  };

  /// <summary>Deserializes a JSON object into a typed Vast DTO.</summary>
  public static T? Deserialize<T>(string json) where T : class =>
    JsonSerializer.Deserialize<T>(json, Options);

  /// <summary>Deserializes either a direct JSON array or an envelope object containing a collection.</summary>
  public static IReadOnlyList<TItem> DeserializeCollection<TItem, TEnvelope>(
    string                               json,
    Func<TEnvelope, IReadOnlyList<TItem>> selectItems)
    where TEnvelope : class
  {
    var trimmedJson = json.TrimStart();
    if (trimmedJson.StartsWith("[", StringComparison.Ordinal))
      return JsonSerializer.Deserialize<List<TItem>>(json, Options) ?? [];

    var envelope = JsonSerializer.Deserialize<TEnvelope>(json, Options);
    return envelope is null ? [] : selectItems(envelope);
  }

  /// <summary>Allows Vast numeric fields to arrive as JSON numbers or numeric strings.</summary>
  private sealed class FlexibleDoubleConverter : JsonConverter<double>
  {
    /// <summary>Reads a number from a JSON number or numeric string.</summary>
    public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      if (reader.TokenType == JsonTokenType.Number && reader.TryGetDouble(out var number))
        return number;

      return reader.TokenType == JsonTokenType.String &&
             double.TryParse(reader.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out number)
        ? number
        : 0;
    }

    /// <summary>Writes normalized numeric values back as JSON numbers.</summary>
    public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options) =>
      writer.WriteNumberValue(value);
  }

  /// <summary>Allows id-like fields to arrive as JSON strings or numbers.</summary>
  private sealed class FlexibleStringConverter : JsonConverter<string>
  {
    /// <summary>Reads a string from a JSON string or number.</summary>
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
      reader.TokenType switch
      {
        JsonTokenType.String => reader.GetString() ?? "",
        JsonTokenType.Number => reader.GetDouble().ToString(CultureInfo.InvariantCulture),
        _ => ""
      };

    /// <summary>Writes string values normally.</summary>
    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) =>
      writer.WriteStringValue(value);
  }
}
