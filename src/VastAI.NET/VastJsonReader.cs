namespace VastAI.NET;

using System.Text.Json;
using Models;
using Serialization;

/// <summary>Reads Vast REST JSON payloads into normalized library models.</summary>
internal static class VastJsonReader
{
  /// <summary>Reads marketplace offers from Vast's direct array or collection envelope responses.</summary>
  public static IReadOnlyList<VastOffer> ReadOffers(string json) =>
    ReadOfferElements(json)
      .Select(ReadOfferElement)
      .Where(offer => !string.IsNullOrWhiteSpace(offer.Id))
      .ToList();

  /// <summary>Reads account instances from Vast's direct array or collection envelope responses.</summary>
  public static IReadOnlyList<VastInstance> ReadInstances(string json) =>
    ReadInstanceElements(json)
      .Select(ReadInstanceElement)
      .Where(instance => !string.IsNullOrWhiteSpace(instance.Id))
      .ToList();

  /// <summary>Reads one instance from Vast's show-instance response.</summary>
  public static VastInstance ReadInstance(string json) =>
    ReadInstances(json).FirstOrDefault() ??
    VastJson.Deserialize<VastInstanceDto>(json)?.ToModel(VastRawJson.FromJson(json)) ??
    VastInstanceDto.Empty.ToModel();

  /// <summary>Reads the new instance id from Vast's create-instance response.</summary>
  public static string ReadNewContract(string json) =>
    VastJson.Deserialize<VastCreateInstanceResponse>(json)?.NewInstanceId ?? "";

  /// <summary>Extracts instance objects from Vast's direct array and known collection wrappers.</summary>
  private static IReadOnlyList<JsonElement> ReadOfferElements(string json) =>
    ReadObjectElements(json, "offers", "results", "instances");

  /// <summary>Extracts instance objects from Vast's direct array and known collection wrappers.</summary>
  private static IReadOnlyList<JsonElement> ReadInstanceElements(string json)
  {
    return ReadObjectElements(json, "instances", "results", "items", "data");
  }

  /// <summary>Extracts JSON objects from a direct array, a known wrapper, or a direct object.</summary>
  private static IReadOnlyList<JsonElement> ReadObjectElements(string json, params string[] wrapperNames)
  {
    using var document = JsonDocument.Parse(json);
    var       root     = document.RootElement;
    if (root.ValueKind == JsonValueKind.Array)
      return root.EnumerateArray().Select(element => element.Clone()).ToList();

    foreach (var name in wrapperNames)
      if (root.TryGetProperty(name, out var child))
      {
        if (child.ValueKind == JsonValueKind.Array)
          return child.EnumerateArray().Select(element => element.Clone()).ToList();

        if (child.ValueKind == JsonValueKind.Object)
          return [child.Clone()];
      }

    return root.ValueKind == JsonValueKind.Object ? [root.Clone()] : [];
  }

  /// <summary>Maps one raw Vast offer object into the public model while preserving its complete JSON object.</summary>
  private static VastOffer ReadOfferElement(JsonElement element)
  {
    var rawJson = element.GetRawText();
    return VastJson.Deserialize<VastOfferDto>(rawJson)?.ToModel(VastRawJson.FromJson(rawJson)) ??
           new VastOffer("", "", 0, 0, 0, 0, 0, 0, 0, 0, 0, "");
  }

  /// <summary>Maps one raw Vast instance object into the public model while preserving its complete JSON object.</summary>
  private static VastInstance ReadInstanceElement(JsonElement element)
  {
    var rawJson = element.GetRawText();
    return VastJson.Deserialize<VastInstanceDto>(rawJson)?.ToModel(VastRawJson.FromJson(rawJson)) ??
           VastInstanceDto.Empty.ToModel();
  }
}
