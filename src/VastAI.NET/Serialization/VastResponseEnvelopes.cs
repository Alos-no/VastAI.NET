namespace VastAI.NET.Serialization;

using System.Text.Json.Serialization;
using static VastResponseHelpers;

/// <summary>Collection wrappers used by Vast offer endpoints.</summary>
internal sealed class VastOfferEnvelope
{
  public List<VastOfferDto>? Offers { get; set; }
  public List<VastOfferDto>? Results { get; set; }
  public List<VastOfferDto>? Instances { get; set; }
  public IReadOnlyList<VastOfferDto> Items => Offers ?? Results ?? Instances ?? [];
}

/// <summary>Collection wrappers used by Vast instance endpoints.</summary>
internal sealed class VastInstanceEnvelope
{
  [JsonConverter(typeof(SingleOrListJsonConverter<VastInstanceDto>))]
  public SingleOrList<VastInstanceDto>? Instances { get; set; }

  [JsonConverter(typeof(SingleOrListJsonConverter<VastInstanceDto>))]
  public SingleOrList<VastInstanceDto>? Results { get; set; }

  [JsonConverter(typeof(SingleOrListJsonConverter<VastInstanceDto>))]
  public SingleOrList<VastInstanceDto>? Items { get; set; }

  [JsonConverter(typeof(SingleOrListJsonConverter<VastInstanceDto>))]
  public SingleOrList<VastInstanceDto>? Data { get; set; }

  public IReadOnlyList<VastInstanceDto> InstancesOrEmpty => Instances ?? Results ?? Items ?? Data ?? [];
}

/// <summary>Create-instance response with supported id aliases.</summary>
internal sealed class VastCreateInstanceResponse
{
  public string NewContract { get; set; } = "";
  public string ContractId { get; set; } = "";
  public string Id { get; set; } = "";
  public string NewInstanceId => FirstNonEmpty(NewContract, ContractId, InstanceId, Id);

  [JsonPropertyName("instance_id")]
  public string InstanceId { get; set; } = "";
}
