namespace VastAI.NET.Serialization;

using Models;
using static VastResponseHelpers;

/// <summary>Raw Vast offer DTO with documented API aliases before normalization.</summary>
internal sealed class VastOfferDto
{
  public string Id { get; set; } = "";
  public string AskContractId { get; set; } = "";
  public string GpuName { get; set; } = "";
  public double DphTotal { get; set; }
  public double Dph { get; set; }
  public double CudaVers { get; set; }
  public double CudaMaxGood { get; set; }
  public double Reliability { get; set; }
  public double DiskSpace { get; set; }
  public double InetUp { get; set; }
  public double InetDown { get; set; }
  public double InternetUpCostPerTb { get; set; }
  public double InternetDownCostPerTb { get; set; }
  public double InetUpCost { get; set; }
  public double InetDownCost { get; set; }
  public double ComputeCap { get; set; }
  public string Geolocation { get; set; } = "";

  /// <summary>Maps raw Vast offer fields to the normalized offer model used by rental filtering.</summary>
  public VastOffer ToModel() => new(
    FirstNonEmpty(Id, AskContractId),
    GpuName,
    FirstPositive(DphTotal, Dph),
    FirstPositive(CudaVers, CudaMaxGood),
    Reliability,
    DiskSpace,
    InetUp,
    InetDown,
    InternetCostPerTb(InternetUpCostPerTb, InetUpCost),
    InternetCostPerTb(InternetDownCostPerTb, InetDownCost),
    (int)Math.Round(ComputeCap, MidpointRounding.AwayFromZero),
    Geolocation);
}
