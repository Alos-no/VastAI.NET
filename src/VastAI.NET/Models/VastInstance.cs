namespace VastAI.NET.Models;

using System.Collections.ObjectModel;
using System.Text.Json.Nodes;

/// <summary>Normalized Vast instance state used for safety checks, status output, SSH discovery, and browser worker recovery.</summary>
public sealed record VastInstance
{
  /// <summary>Creates a normalized Vast instance model from the fields most callers need directly.</summary>
  public VastInstance(
    string                                  id,
    string                                  status,
    string                                  label,
    string                                  gpuName,
    double                                  price,
    double                                  ageHours,
    string                                  sshHost          = "",
    int                                     sshPort          = 0,
    string                                  sshUser          = "root",
    string                                  publicIpAddress  = "",
    IReadOnlyDictionary<string, string>?    extraEnvironment = null,
    IReadOnlyList<VastPortMapping>?         ports            = null,
    VastRawJson?                            raw              = null)
  {
    Id               = id;
    Status           = status;
    Label            = label;
    GpuName          = gpuName;
    Price            = price;
    AgeHours         = ageHours;
    SshHost          = sshHost;
    SshPort          = sshPort;
    SshUser          = sshUser;
    PublicIpAddress  = publicIpAddress;
    ExtraEnvironment = new ReadOnlyDictionary<string, string>(
      new Dictionary<string, string>(extraEnvironment ?? new Dictionary<string, string>(), StringComparer.Ordinal));
    Ports = ports?.ToArray() ?? [];
    Raw   = raw ?? VastRawJson.Empty;
  }

  /// <summary>Vast instance or contract id.</summary>
  public string Id { get; init; }

  /// <summary>Vast-reported instance state, such as running, loading, stopped, or destroyed.</summary>
  public string Status { get; init; }

  /// <summary>User-supplied Vast label assigned when the instance was created.</summary>
  public string Label { get; init; }

  /// <summary>Vast-reported GPU name.</summary>
  public string GpuName { get; init; }

  /// <summary>Hourly price in USD reported by Vast.</summary>
  public double Price { get; init; }

  /// <summary>Estimated rental age in hours.</summary>
  public double AgeHours { get; init; }

  /// <summary>SSH host reported by Vast when direct SSH metadata is available.</summary>
  public string SshHost { get; init; }

  /// <summary>SSH port reported by Vast when direct SSH metadata is available.</summary>
  public int SshPort { get; init; }

  /// <summary>SSH user reported by Vast, defaulting to root for Vast containers.</summary>
  public string SshUser { get; init; }

  /// <summary>Public IPv4 address reported by Vast for direct networking and exposed application ports.</summary>
  public string PublicIpAddress { get; init; }

  /// <summary>Environment values returned by Vast as extra_env for the rented instance.</summary>
  public IReadOnlyDictionary<string, string> ExtraEnvironment { get; init; }

  /// <summary>Application and service port mappings returned by Vast for the rented instance.</summary>
  public IReadOnlyList<VastPortMapping> Ports { get; init; }

  /// <summary>Full raw Vast instance object so callers can inspect new Vast fields before the library adds first-class properties.</summary>
  public VastRawJson Raw { get; init; }

  /// <summary>Whether this instance should block a new paid rental unless the user explicitly overrides the gate.</summary>
  public bool IsActive => Status.Equals("running", StringComparison.OrdinalIgnoreCase) ||
    Status.Equals("loading", StringComparison.OrdinalIgnoreCase) ||
    Status.Equals("starting", StringComparison.OrdinalIgnoreCase);

  /// <summary>Estimated spend so far using Vast's reported hourly price and age.</summary>
  public double EstimatedCost => Math.Max(0, Price * AgeHours);

  /// <summary>Whether Vast has reported enough SSH metadata to connect directly.</summary>
  public bool HasSshTarget => !string.IsNullOrWhiteSpace(SshHost) && SshPort > 0;

  /// <summary>Reads one environment value returned by Vast in extra_env.</summary>
  public bool TryGetExtraEnvironmentValue(string name, out string value) =>
    ExtraEnvironment.TryGetValue(name, out value!);

  /// <summary>Resolves an exposed HTTP URI for a container port using Vast's public IP and port mapping data.</summary>
  public bool TryGetPublicUriForContainerPort(int containerPort, out Uri uri)
  {
    var mapping = Ports.FirstOrDefault(port => port.ContainerPort == containerPort);
    if (mapping is null || mapping.HostPort <= 0)
    {
      uri = null!;
      return false;
    }

    var host = string.IsNullOrWhiteSpace(mapping.Host) || mapping.Host is "0.0.0.0" or "::"
      ? PublicIpAddress
      : mapping.Host;
    if (string.IsNullOrWhiteSpace(host))
    {
      uri = null!;
      return false;
    }

    uri = new UriBuilder("http", host, mapping.HostPort).Uri;
    return true;
  }
}

/// <summary>One Vast port mapping from a container port to a public host port.</summary>
/// <param name="ContainerPort">Port inside the Vast container.</param>
/// <param name="Protocol">Port protocol, normally tcp.</param>
/// <param name="Host">Public or wildcard host reported by Vast.</param>
/// <param name="HostPort">Host-side port exposed by Vast.</param>
public sealed record VastPortMapping(int ContainerPort, string Protocol, string Host, int HostPort);

/// <summary>Full raw Vast JSON object for callers that need fields not yet normalized by this library.</summary>
public sealed class VastRawJson
{
  /// <summary>Empty raw object used when no Vast JSON object was available.</summary>
  public static VastRawJson Empty { get; } = new(new JsonObject());

  /// <summary>Creates a raw JSON wrapper around a cloned Vast object.</summary>
  public VastRawJson(JsonObject value)
  {
    Object = value;
  }

  /// <summary>Raw Vast object, including normalized and not-yet-modeled fields.</summary>
  public JsonObject Object { get; }

  /// <summary>Reads a top-level property from the raw Vast object.</summary>
  public bool TryGetProperty(string name, out JsonNode? value) =>
    Object.TryGetPropertyValue(name, out value);

  /// <summary>Returns the raw Vast object as compact JSON.</summary>
  public override string ToString() =>
    Object.ToJsonString();

  /// <summary>Clones one Vast JSON object into a public raw wrapper.</summary>
  internal static VastRawJson FromJson(string json) =>
    new(JsonNode.Parse(json)?.AsObject() ?? new JsonObject());
}
