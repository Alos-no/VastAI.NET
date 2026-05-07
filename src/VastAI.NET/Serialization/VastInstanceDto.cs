namespace VastAI.NET.Serialization;

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

  /// <summary>Maps raw Vast instance fields to the normalized model used by safety and SSH discovery.</summary>
  public VastInstance ToModel()
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
      FirstNonEmpty(SshUser, User, "root"));
  }
}
