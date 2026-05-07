namespace VastAI.NET.Models;

/// <summary>Normalized Vast instance state used for safety checks, status output, and SSH target discovery.</summary>
public sealed record VastInstance(
  string Id,
  string Status,
  string Label,
  string GpuName,
  double Price,
  double AgeHours,
  string SshHost = "",
  int    SshPort = 0,
  string SshUser = "root")
{
  /// <summary>Whether this instance should block a new paid rental unless the user explicitly overrides the gate.</summary>
  public bool IsActive => Status.Equals("running", StringComparison.OrdinalIgnoreCase) ||
    Status.Equals("loading", StringComparison.OrdinalIgnoreCase) ||
    Status.Equals("starting", StringComparison.OrdinalIgnoreCase);

  /// <summary>Estimated spend so far using Vast's reported hourly price and age.</summary>
  public double EstimatedCost => Math.Max(0, Price * AgeHours);

  /// <summary>Whether Vast has reported enough SSH metadata to connect directly.</summary>
  public bool HasSshTarget => !string.IsNullOrWhiteSpace(SshHost) && SshPort > 0;
}
