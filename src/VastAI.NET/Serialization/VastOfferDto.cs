namespace VastAI.NET.Serialization;

using System.Globalization;
using System.Text.Json;
using Models;
using static VastResponseHelpers;

/// <summary>Raw Vast offer DTO with documented API aliases before normalization.</summary>
internal sealed class VastOfferDto
{
  public string Id { get; set; } = "";
  public string AskContractId { get; set; } = "";
  public string BundleId { get; set; } = "";
  public string BundledResults { get; set; } = "";
  public double BwNvlink { get; set; }
  public string CpuArch { get; set; } = "";
  public double CpuCores { get; set; }
  public double CpuCoresEffective { get; set; }
  public double CpuGhz { get; set; }
  public string CpuName { get; set; } = "";
  public double CpuRam { get; set; }
  public double CreditDiscountMax { get; set; }
  public string GpuName { get; set; } = "";
  public double DphTotal { get; set; }
  public double Dph { get; set; }
  public double DphBase { get; set; }
  public double CudaVers { get; set; }
  public double CudaMaxGood { get; set; }
  public int DirectPortCount { get; set; }
  public double Reliability { get; set; }
  public double ReliabilityMult { get; set; }
  public double Reliability2 { get; set; }
  public double DiskSpace { get; set; }
  public double DiskBw { get; set; }
  public string DiskName { get; set; } = "";
  public double Dlperf { get; set; }
  public double DlperfPerDphtotal { get; set; }
  public string DriverVersion { get; set; } = "";
  public double DriverVers { get; set; }
  public double Duration { get; set; }
  public double EndDate { get; set; }
  public string External { get; set; } = "";
  public double FlopsPerDphtotal { get; set; }
  public double Geolocode { get; set; }
  public string GpuArch { get; set; } = "";
  public bool GpuDisplayActive { get; set; }
  public double GpuFrac { get; set; }
  public JsonElement GpuIds { get; set; }
  public int GpuLanes { get; set; }
  public double GpuMemBw { get; set; }
  public double GpuRam { get; set; }
  public double GpuTotalRam { get; set; }
  public double GpuMaxPower { get; set; }
  public double GpuMaxTemp { get; set; }
  public double HasAvx { get; set; }
  public string HostId { get; set; } = "";
  public int HostingType { get; set; }
  public string Hostname { get; set; } = "";
  public double InetUp { get; set; }
  public double InetDown { get; set; }
  public double InternetUpCostPerTb { get; set; }
  public double InternetDownCostPerTb { get; set; }
  public double InetUpCost { get; set; }
  public double InetDownCost { get; set; }
  public bool IsBid { get; set; }
  public string Logo { get; set; } = "";
  public string MachineId { get; set; } = "";
  public double MinBid { get; set; }
  public string MoboName { get; set; } = "";
  public int NumGpus { get; set; }
  public string OsVersion { get; set; } = "";
  public double PciGen { get; set; }
  public double PcieBw { get; set; }
  public string PublicIpaddr { get; set; } = "";
  public double ComputeCap { get; set; }
  public string Geolocation { get; set; } = "";
  public bool Rentable { get; set; }
  public bool Rented { get; set; }
  public double Score { get; set; }
  public double StartDate { get; set; }
  public bool StaticIp { get; set; }
  public double StorageCost { get; set; }
  public double StorageTotalCost { get; set; }
  public double TotalFlops { get; set; }
  public string Verification { get; set; } = "";
  public int Vericode { get; set; }
  public double VramCostperhour { get; set; }
  public string Webpage { get; set; } = "";
  public bool VmsEnabled { get; set; }
  public double ExpectedReliability { get; set; }
  public bool IsVmDeverified { get; set; }
  public string ResourceType { get; set; } = "";
  public string ClusterId { get; set; } = "";
  public string AvailVolAskId { get; set; } = "";
  public double AvailVolDph { get; set; }
  public double AvailVolSize { get; set; }
  public double NwDiskMinBw { get; set; }
  public double NwDiskMaxBw { get; set; }
  public double NwDiskAvgBw { get; set; }
  public int Rn { get; set; }
  public double DphTotalAdj { get; set; }
  public double DiscountRate { get; set; }
  public double DiscountedHourly { get; set; }
  public double DiscountedDphTotal { get; set; }
  public string TimeRemaining { get; set; } = "";
  public string TimeRemainingIsbid { get; set; } = "";
  public JsonElement Search { get; set; }
  public JsonElement Instance { get; set; }

  /// <summary>Maps raw Vast offer fields to the normalized offer model used by rental filtering.</summary>
  public VastOffer ToModel(VastRawJson? raw = null) => new(
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
    Geolocation,
    raw)
  {
    AskContractId            = AskContractId,
    BundleId                 = BundleId,
    BundledResults           = BundledResults,
    BwNvlinkGbPerSecond      = BwNvlink,
    CpuArch                  = CpuArch,
    CpuCores                 = CpuCores,
    CpuCoresEffective        = CpuCoresEffective,
    CpuGhz                   = CpuGhz,
    CpuName                  = CpuName,
    CpuRamGb                 = NormalizeMemoryGb(CpuRam),
    CreditDiscountMax        = CreditDiscountMax,
    CudaMaxGood              = FirstPositive(CudaMaxGood, CudaVers),
    DirectPortCount          = DirectPortCount,
    DiskBwMbPerSecond        = DiskBw,
    DiskName                 = DiskName,
    DlPerf                   = Dlperf,
    DlPerfPerDollarHour      = DlperfPerDphtotal,
    DphBase                  = DphBase,
    DphTotal                 = FirstPositive(DphTotal, Dph),
    DriverVersion            = DriverVersion,
    DriverVersionNumber      = DriverVers,
    DurationSeconds          = Duration,
    EndDateUnixSeconds       = EndDate,
    External                 = External,
    FlopsPerDollarHour       = FlopsPerDphtotal,
    Geolocode                = Geolocode,
    GpuArch                  = GpuArch,
    GpuDisplayActive         = GpuDisplayActive,
    GpuFrac                  = GpuFrac,
    GpuIds                   = ReadIntArray(GpuIds),
    GpuLanes                 = GpuLanes,
    GpuMemBwGbPerSecond      = GpuMemBw,
    GpuRamGb                 = NormalizeMemoryGb(GpuRam),
    GpuTotalRamGb            = NormalizeMemoryGb(GpuTotalRam),
    GpuMaxPowerWatts         = GpuMaxPower,
    GpuMaxTempCelsius        = GpuMaxTemp,
    HasAvx                   = HasAvx > 0,
    HostId                   = HostId,
    HostingType              = HostingType,
    Hostname                 = Hostname,
    IsBid                    = IsBid,
    Logo                     = Logo,
    MachineId                = MachineId,
    MinBid                   = MinBid,
    MoboName                 = MoboName,
    NumGpus                  = NumGpus,
    OsVersion                = OsVersion,
    PciGen                   = PciGen,
    PcieBwGbPerSecond        = PcieBw,
    PublicIpAddress          = PublicIpaddr,
    ReliabilityMult          = ReliabilityMult,
    Rentable                 = Rentable,
    Rented                   = Rented,
    Score                    = Score,
    StartDateUnixSeconds     = StartDate,
    StaticIp                 = StaticIp,
    StorageCost              = StorageCost,
    StorageTotalCost         = StorageTotalCost,
    TotalFlops               = TotalFlops,
    Verification             = Verification,
    VeriCode                 = Vericode,
    VramCostPerHour          = VramCostperhour,
    Webpage                  = Webpage,
    VmsEnabled               = VmsEnabled,
    ExpectedReliability      = ExpectedReliability,
    IsVmDeverified           = IsVmDeverified,
    ResourceType             = ResourceType,
    ClusterId                = ClusterId,
    AvailVolAskId            = AvailVolAskId,
    AvailVolDph              = AvailVolDph,
    AvailVolSizeGb           = AvailVolSize,
    NwDiskMinBw              = NwDiskMinBw,
    NwDiskMaxBw              = NwDiskMaxBw,
    NwDiskAvgBw              = NwDiskAvgBw,
    Rn                       = Rn,
    DphTotalAdjusted         = DphTotalAdj,
    Reliability2             = Reliability2,
    DiscountRate             = DiscountRate,
    DiscountedHourly         = DiscountedHourly,
    DiscountedDphTotal       = DiscountedDphTotal,
    TimeRemaining            = TimeRemaining,
    TimeRemainingIsBid       = TimeRemainingIsbid,
    SearchPricing            = ReadPricing(Search),
    InstancePricing          = ReadPricing(Instance)
  };

  /// <summary>Normalizes Vast memory fields that may arrive as MB or GB depending on the endpoint and API version.</summary>
  private static double NormalizeMemoryGb(double memory) =>
    memory > 1024 ? memory / 1024 : memory;

  /// <summary>Reads Vast integer arrays, accepting numeric strings when hosts return loose JSON values.</summary>
  private static IReadOnlyList<int> ReadIntArray(JsonElement element)
  {
    if (element.ValueKind != JsonValueKind.Array)
      return [];

    return element.EnumerateArray()
      .Select(ReadInt)
      .Where(value => value > 0)
      .ToList();
  }

  /// <summary>Reads nested Vast price breakdowns from search and instance objects.</summary>
  private static VastOfferPricing? ReadPricing(JsonElement element)
  {
    if (element.ValueKind != JsonValueKind.Object)
      return null;

    return new VastOfferPricing(
      ReadDoubleProperty(element, "gpuCostPerHour", "gpu_cost_per_hour"),
      ReadDoubleProperty(element, "diskHour", "disk_hour"),
      ReadDoubleProperty(element, "totalHour", "total_hour"),
      ReadDoubleProperty(element, "discountTotalHour", "discount_total_hour"),
      ReadDoubleProperty(element, "discountedTotalPerHour", "discounted_total_per_hour"));
  }

  /// <summary>Reads the first matching double property from a JSON object.</summary>
  private static double ReadDoubleProperty(JsonElement element, params string[] names)
  {
    foreach (var name in names)
      if (element.TryGetProperty(name, out var property))
        return ReadDouble(property);

    return 0;
  }

  /// <summary>Reads one JSON number or numeric string as a double.</summary>
  private static double ReadDouble(JsonElement value)
  {
    if (value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var number))
      return number;

    return double.TryParse(value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out number) ? number : 0;
  }

  /// <summary>Reads one JSON number or numeric string as an integer.</summary>
  private static int ReadInt(JsonElement value)
  {
    if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number))
      return number;

    return int.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out number) ? number : 0;
  }
}
