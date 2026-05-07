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
  string Country);
