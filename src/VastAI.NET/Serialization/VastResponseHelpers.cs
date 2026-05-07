namespace VastAI.NET.Serialization;

/// <summary>Shared normalization helpers for Vast API aliases.</summary>
internal static class VastResponseHelpers
{
  /// <summary>Returns the first non-empty string from a set of API alias values.</summary>
  public static string FirstNonEmpty(params string[] values) =>
    values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? "";

  /// <summary>Returns the first positive numeric value from a set of API alias values.</summary>
  public static double FirstPositive(params double[] values) =>
    values.FirstOrDefault(value => value > 0);

  /// <summary>Normalizes Vast bandwidth pricing to dollars per TB.</summary>
  public static double InternetCostPerTb(double perTb, double perGb) =>
    perTb > 0 ? perTb : perGb > 0 ? perGb * 1024 : double.PositiveInfinity;
}
