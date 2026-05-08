namespace VastAI.NET.Models;

/// <summary>Normalized Vast marketplace offer used for search output, rental filtering, and paid-operation safety checks.</summary>
public sealed record VastOffer(
  string Id,
  string GpuName,
  double Price,
  double Cuda,
  double Reliability,
  double Disk,
  double InetUp,
  double InetDown,
  double InternetUpCostPerTb,
  double InternetDownCostPerTb,
  int    ComputeCapability,
  string Country,
  VastRawJson? Raw = null)
{
  /// <summary>Full raw Vast offer object so callers can inspect fields not yet normalized by this library.</summary>
  public VastRawJson Raw { get; init; } = Raw ?? VastRawJson.Empty;

  /// <summary>Vast ask contract id. This is usually the id passed to create-instance.</summary>
  public string AskContractId { get; init; } = "";

  /// <summary>Vast bundle id when the search endpoint returns one.</summary>
  public string BundleId { get; init; } = "";

  /// <summary>Raw bundled_results value when Vast returns it.</summary>
  public string BundledResults { get; init; } = "";

  /// <summary>NVLink bandwidth in GB/s.</summary>
  public double BwNvlinkGbPerSecond { get; init; }

  /// <summary>Host CPU architecture, such as amd64 or arm64.</summary>
  public string CpuArch { get; init; } = "";

  /// <summary>Total virtual CPU cores reported by Vast.</summary>
  public double CpuCores { get; init; }

  /// <summary>Effective CPU cores allocated to this offer.</summary>
  public double CpuCoresEffective { get; init; }

  /// <summary>CPU clock speed in GHz.</summary>
  public double CpuGhz { get; init; }

  /// <summary>Host CPU model name.</summary>
  public string CpuName { get; init; } = "";

  /// <summary>System RAM in GB, normalized from Vast payloads that may report MB or GB.</summary>
  public double CpuRamGb { get; init; }

  /// <summary>Maximum credit discount rate for the offer.</summary>
  public double CreditDiscountMax { get; init; }

  /// <summary>Maximum CUDA version considered usable for the offer.</summary>
  public double CudaMaxGood { get; init; }

  /// <summary>Number of direct exposed ports available on the host.</summary>
  public int DirectPortCount { get; init; }

  /// <summary>Disk read bandwidth in MB/s.</summary>
  public double DiskBwMbPerSecond { get; init; }

  /// <summary>Disk or storage model name.</summary>
  public string DiskName { get; init; } = "";

  /// <summary>Vast DLPerf score.</summary>
  public double DlPerf { get; init; }

  /// <summary>Vast DLPerf per dollar per hour score.</summary>
  public double DlPerfPerDollarHour { get; init; }

  /// <summary>Base hourly price before storage and other adjustments.</summary>
  public double DphBase { get; init; }

  /// <summary>Total hourly price returned by Vast.</summary>
  public double DphTotal { get; init; }

  /// <summary>NVIDIA driver version string.</summary>
  public string DriverVersion { get; init; } = "";

  /// <summary>Numeric driver version when Vast returns it.</summary>
  public double DriverVersionNumber { get; init; }

  /// <summary>Offer duration in seconds.</summary>
  public double DurationSeconds { get; init; }

  /// <summary>Offer end date as a Unix timestamp when Vast returns it.</summary>
  public double EndDateUnixSeconds { get; init; }

  /// <summary>Raw external-offer marker returned by Vast.</summary>
  public string External { get; init; } = "";

  /// <summary>FLOPs per dollar per hour.</summary>
  public double FlopsPerDollarHour { get; init; }

  /// <summary>Raw geolocation code returned by Vast.</summary>
  public double Geolocode { get; init; }

  /// <summary>GPU architecture, normally nvidia or amd.</summary>
  public string GpuArch { get; init; } = "";

  /// <summary>Whether the host GPU has an attached active display.</summary>
  public bool GpuDisplayActive { get; init; }

  /// <summary>Fraction of the GPU resources offered.</summary>
  public double GpuFrac { get; init; }

  /// <summary>GPU ids included in this offer.</summary>
  public IReadOnlyList<int> GpuIds { get; init; } = [];

  /// <summary>PCIe lanes available to the GPU.</summary>
  public int GpuLanes { get; init; }

  /// <summary>GPU memory bandwidth in GB/s.</summary>
  public double GpuMemBwGbPerSecond { get; init; }

  /// <summary>GPU VRAM in GB, normalized from Vast payloads that may report MB or GB.</summary>
  public double GpuRamGb { get; init; }

  /// <summary>Total GPU VRAM across all GPUs in GB, normalized from Vast payloads that may report MB or GB.</summary>
  public double GpuTotalRamGb { get; init; }

  /// <summary>GPU power limit in watts.</summary>
  public double GpuMaxPowerWatts { get; init; }

  /// <summary>GPU temperature limit in Celsius.</summary>
  public double GpuMaxTempCelsius { get; init; }

  /// <summary>Whether the CPU supports AVX.</summary>
  public bool HasAvx { get; init; }

  /// <summary>Vast host id.</summary>
  public string HostId { get; init; } = "";

  /// <summary>Vast hosting type code.</summary>
  public int HostingType { get; init; }

  /// <summary>Host name when Vast exposes it.</summary>
  public string Hostname { get; init; } = "";

  /// <summary>Whether this offer is bid/interruptible pricing.</summary>
  public bool IsBid { get; init; }

  /// <summary>Host logo path returned by Vast.</summary>
  public string Logo { get; init; } = "";

  /// <summary>Vast machine id.</summary>
  public string MachineId { get; init; } = "";

  /// <summary>Minimum bid price for interruptible rentals.</summary>
  public double MinBid { get; init; }

  /// <summary>Motherboard model name when Vast reports it.</summary>
  public string MoboName { get; init; } = "";

  /// <summary>Number of GPUs in the offer.</summary>
  public int NumGpus { get; init; }

  /// <summary>Host OS version returned by Vast.</summary>
  public string OsVersion { get; init; } = "";

  /// <summary>PCIe generation.</summary>
  public double PciGen { get; init; }

  /// <summary>PCIe bandwidth in GB/s.</summary>
  public double PcieBwGbPerSecond { get; init; }

  /// <summary>Public IP address when Vast returns it on an offer.</summary>
  public string PublicIpAddress { get; init; } = "";

  /// <summary>Vast reliability multiplier.</summary>
  public double ReliabilityMult { get; init; }

  /// <summary>Whether the offer is rentable.</summary>
  public bool Rentable { get; init; }

  /// <summary>Whether the offer is already rented.</summary>
  public bool Rented { get; init; }

  /// <summary>Vast search score.</summary>
  public double Score { get; init; }

  /// <summary>Offer start date as a Unix timestamp when Vast returns it.</summary>
  public double StartDateUnixSeconds { get; init; }

  /// <summary>Whether the offer includes a static IP.</summary>
  public bool StaticIp { get; init; }

  /// <summary>Storage hourly cost.</summary>
  public double StorageCost { get; init; }

  /// <summary>Total storage hourly cost.</summary>
  public double StorageTotalCost { get; init; }

  /// <summary>Total FLOPs reported by Vast.</summary>
  public double TotalFlops { get; init; }

  /// <summary>Verification status string, such as verified.</summary>
  public string Verification { get; init; } = "";

  /// <summary>Verification code returned by Vast.</summary>
  public int VeriCode { get; init; }

  /// <summary>VRAM hourly cost.</summary>
  public double VramCostPerHour { get; init; }

  /// <summary>Host webpage returned by Vast.</summary>
  public string Webpage { get; init; } = "";

  /// <summary>Whether the host has VMs enabled.</summary>
  public bool VmsEnabled { get; init; }

  /// <summary>Expected reliability score when Vast returns it.</summary>
  public double ExpectedReliability { get; init; }

  /// <summary>Whether Vast has VM-deverified the machine.</summary>
  public bool IsVmDeverified { get; init; }

  /// <summary>Vast resource type, normally gpu.</summary>
  public string ResourceType { get; init; } = "";

  /// <summary>Cluster id returned by Vast.</summary>
  public string ClusterId { get; init; } = "";

  /// <summary>Available volume ask id.</summary>
  public string AvailVolAskId { get; init; } = "";

  /// <summary>Available volume hourly price.</summary>
  public double AvailVolDph { get; init; }

  /// <summary>Available volume size in GB.</summary>
  public double AvailVolSizeGb { get; init; }

  /// <summary>Minimum network disk bandwidth.</summary>
  public double NwDiskMinBw { get; init; }

  /// <summary>Maximum network disk bandwidth.</summary>
  public double NwDiskMaxBw { get; init; }

  /// <summary>Average network disk bandwidth.</summary>
  public double NwDiskAvgBw { get; init; }

  /// <summary>Row number returned by Vast search.</summary>
  public int Rn { get; init; }

  /// <summary>Adjusted total hourly price.</summary>
  public double DphTotalAdjusted { get; init; }

  /// <summary>Second reliability score returned by Vast.</summary>
  public double Reliability2 { get; init; }

  /// <summary>Discount rate returned by Vast.</summary>
  public double DiscountRate { get; init; }

  /// <summary>Discounted hourly price returned by Vast.</summary>
  public double DiscountedHourly { get; init; }

  /// <summary>Discounted total hourly price returned by Vast.</summary>
  public double DiscountedDphTotal { get; init; }

  /// <summary>Time remaining text for fixed-duration or bid offers.</summary>
  public string TimeRemaining { get; init; } = "";

  /// <summary>Bid time remaining text returned by Vast.</summary>
  public string TimeRemainingIsBid { get; init; } = "";

  /// <summary>Nested search pricing details returned by Vast.</summary>
  public VastOfferPricing? SearchPricing { get; init; }

  /// <summary>Nested instance pricing details returned by Vast.</summary>
  public VastOfferPricing? InstancePricing { get; init; }
}

/// <summary>Nested Vast pricing breakdown used by offer search and instance pricing objects.</summary>
public sealed record VastOfferPricing(
  double GpuCostPerHour,
  double DiskHour,
  double TotalHour,
  double DiscountTotalHour,
  double DiscountedTotalPerHour);
