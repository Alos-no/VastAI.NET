namespace VastAI.NET;

using Models;
using Serialization;

/// <summary>Reads Vast REST JSON payloads into normalized library models.</summary>
internal static class VastJsonReader
{
  /// <summary>Reads marketplace offers from Vast's direct array or collection envelope responses.</summary>
  public static IReadOnlyList<VastOffer> ReadOffers(string json) =>
    VastJson.DeserializeCollection<VastOfferDto, VastOfferEnvelope>(json, envelope => envelope.Items)
            .Select(offer => offer.ToModel())
            .Where(offer => !string.IsNullOrWhiteSpace(offer.Id))
            .ToList();

  /// <summary>Reads account instances from Vast's direct array or collection envelope responses.</summary>
  public static IReadOnlyList<VastInstance> ReadInstances(string json) =>
    VastJson.DeserializeCollection<VastInstanceDto, VastInstanceEnvelope>(json, envelope => envelope.InstancesOrEmpty)
            .Select(instance => instance.ToModel())
            .Where(instance => !string.IsNullOrWhiteSpace(instance.Id))
            .ToList();

  /// <summary>Reads one instance from Vast's show-instance response.</summary>
  public static VastInstance ReadInstance(string json) =>
    ReadInstances(json).FirstOrDefault() ??
    VastJson.Deserialize<VastInstanceDto>(json)?.ToModel() ??
    VastInstanceDto.Empty.ToModel();

  /// <summary>Reads the new instance id from Vast's create-instance response.</summary>
  public static string ReadNewContract(string json) =>
    VastJson.Deserialize<VastCreateInstanceResponse>(json)?.NewInstanceId ?? "";
}
