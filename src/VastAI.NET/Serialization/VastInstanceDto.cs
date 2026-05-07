namespace VastAI.NET.Serialization;

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Models;
using static VastResponseHelpers;

/// <summary>Raw Vast instance DTO with documented API aliases before normalization.</summary>
internal sealed class VastInstanceDto
{
  public static VastInstanceDto Empty { get; } = new();

  public string Id { get; set; } = "";
  public string ContractId { get; set; } = "";
  public string InstanceId { get; set; } = "";
  public string ActualStatus { get; set; } = "";
  public string Status { get; set; } = "";
  public string ContainerState { get; set; } = "";
  public string Label { get; set; } = "";
  public string Name { get; set; } = "";
  public string GpuName { get; set; } = "";
  public string Gpu { get; set; } = "";
  public double DphTotal { get; set; }
  public double Dph { get; set; }
  public double Price { get; set; }
  [JsonPropertyName("totalHour")]
  public double TotalHour { get; set; }
  public double AgeHours { get; set; }
  public double AgeHrs { get; set; }
  public double DurationHours { get; set; }
  public double StartDate { get; set; }
  public double CreatedAt { get; set; }
  public double CreationDate { get; set; }
  public double RentedAt { get; set; }
  public string SshHost { get; set; } = "";
  public string DirectSshHost { get; set; } = "";
  public string PublicIpaddr { get; set; } = "";
  public double SshPort { get; set; }
  public double DirectSshPort { get; set; }
  public string SshUser { get; set; } = "";
  public string User { get; set; } = "";
  public JsonElement ExtraEnv { get; set; }
  public JsonElement Ports { get; set; }

  /// <summary>Maps raw Vast instance fields to the normalized model used by safety and SSH discovery.</summary>
  public VastInstance ToModel(VastRawJson? raw = null)
  {
    var ageHours = FirstPositive(AgeHours, AgeHrs, DurationHours);
    var start    = FirstPositive(StartDate, CreatedAt, CreationDate, RentedAt);
    if (ageHours <= 0 && start > 1_000_000_000)
      ageHours = Math.Max(0, (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - start) / 3600.0);

    return new VastInstance(
      FirstNonEmpty(Id, ContractId, InstanceId),
      FirstNonEmpty(ActualStatus, Status, ContainerState, "unknown"),
      FirstNonEmpty(Label, Name),
      FirstNonEmpty(GpuName, Gpu),
      FirstPositive(DphTotal, Dph, Price, TotalHour),
      ageHours,
      FirstNonEmpty(SshHost, DirectSshHost, PublicIpaddr),
      (int)FirstPositive(SshPort, DirectSshPort),
      FirstNonEmpty(SshUser, User, "root"),
      PublicIpaddr,
      ReadExtraEnvironment(ExtraEnv),
      ReadPorts(Ports),
      raw);
  }

  /// <summary>Normalizes Vast extra_env when it arrives as arrays or objects.</summary>
  private static IReadOnlyDictionary<string, string> ReadExtraEnvironment(JsonElement extraEnv)
  {
    var values = new Dictionary<string, string>(StringComparer.Ordinal);
    if (extraEnv.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
      return values;

    if (extraEnv.ValueKind == JsonValueKind.Object)
    {
      foreach (var property in extraEnv.EnumerateObject())
        values[property.Name] = property.Value.ToString();
      return values;
    }

    if (extraEnv.ValueKind != JsonValueKind.Array)
      return values;

    foreach (var item in extraEnv.EnumerateArray())
      if (TryReadExtraEnvironmentItem(item, out var name, out var value))
        values[name] = value;

    return values;
  }

  /// <summary>Reads one Vast extra_env item from the documented array shape or an object alias shape.</summary>
  private static bool TryReadExtraEnvironmentItem(JsonElement item, out string name, out string value)
  {
    name  = "";
    value = "";
    if (item.ValueKind == JsonValueKind.Array && item.GetArrayLength() >= 2)
    {
      name  = item[0].ToString();
      value = item[1].ToString();
      return !string.IsNullOrWhiteSpace(name);
    }

    if (item.ValueKind != JsonValueKind.Object)
      return false;

    name  = ReadStringProperty(item, "name", "key");
    value = ReadStringProperty(item, "value");
    return !string.IsNullOrWhiteSpace(name);
  }

  /// <summary>Normalizes Vast ports when they arrive as Docker-style objects or arrays of mapping objects.</summary>
  private static IReadOnlyList<VastPortMapping> ReadPorts(JsonElement ports)
  {
    if (ports.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
      return [];

    if (ports.ValueKind == JsonValueKind.Array)
      return ports.EnumerateArray().Select(ReadArrayPortMapping).Where(mapping => mapping is not null).Cast<VastPortMapping>().ToList();

    if (ports.ValueKind != JsonValueKind.Object)
      return [];

    var mappings = new List<VastPortMapping>();
    foreach (var property in ports.EnumerateObject())
    {
      var (containerPort, protocol) = ParsePortKey(property.Name);
      if (containerPort <= 0)
        continue;

      if (property.Value.ValueKind == JsonValueKind.Array)
        mappings.AddRange(property.Value.EnumerateArray()
                                  .Select(item => ReadDockerPortMapping(containerPort, protocol, item))
                                  .Where(mapping => mapping is not null)
                                  .Cast<VastPortMapping>());
      else
      {
        var mapping = ReadDockerPortMapping(containerPort, protocol, property.Value);
        if (mapping is not null)
          mappings.Add(mapping);
      }
    }

    return mappings;
  }

  /// <summary>Reads one array-style Vast port mapping.</summary>
  private static VastPortMapping? ReadArrayPortMapping(JsonElement item)
  {
    if (item.ValueKind != JsonValueKind.Object)
      return null;

    var containerPort = ReadIntProperty(item, "container_port", "containerPort", "port", "private_port");
    var hostPort      = ReadIntProperty(item, "host_port", "hostPort", "public_port");
    if (containerPort <= 0 || hostPort <= 0)
      return null;

    return new VastPortMapping(
      containerPort,
      FirstNonEmpty(ReadStringProperty(item, "protocol", "proto"), "tcp"),
      ReadStringProperty(item, "host", "host_ip", "HostIp", "ip"),
      hostPort);
  }

  /// <summary>Reads one Docker-style Vast port mapping value.</summary>
  private static VastPortMapping? ReadDockerPortMapping(int containerPort, string protocol, JsonElement item)
  {
    if (item.ValueKind is JsonValueKind.Number or JsonValueKind.String)
    {
      var scalarPort = ReadInt(item);
      return scalarPort <= 0 ? null : new VastPortMapping(containerPort, protocol, "", scalarPort);
    }

    if (item.ValueKind != JsonValueKind.Object)
      return null;

    var hostPort = ReadIntProperty(item, "HostPort", "host_port", "hostPort", "public_port");
    if (hostPort <= 0)
      return null;

    return new VastPortMapping(
      containerPort,
      protocol,
      ReadStringProperty(item, "HostIp", "host_ip", "host", "ip"),
      hostPort);
  }

  /// <summary>Reads the container port and protocol from a Docker-style key like 8088/tcp.</summary>
  private static (int ContainerPort, string Protocol) ParsePortKey(string key)
  {
    var parts = key.Split('/', 2, StringSplitOptions.TrimEntries);
    return (int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var port) ? port : 0,
      parts.Length == 2 && !string.IsNullOrWhiteSpace(parts[1]) ? parts[1] : "tcp");
  }

  /// <summary>Reads the first matching string property from a JSON object.</summary>
  private static string ReadStringProperty(JsonElement item, params string[] names)
  {
    foreach (var name in names)
      if (item.TryGetProperty(name, out var property))
        return property.ToString();

    return "";
  }

  /// <summary>Reads the first matching integer property from a JSON object.</summary>
  private static int ReadIntProperty(JsonElement item, params string[] names)
  {
    foreach (var name in names)
      if (item.TryGetProperty(name, out var property))
        return ReadInt(property);

    return 0;
  }

  /// <summary>Reads a JSON number or numeric string as an integer.</summary>
  private static int ReadInt(JsonElement value)
  {
    if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number))
      return number;

    return int.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out number) ? number : 0;
  }
}
