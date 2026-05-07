namespace VastAI.NET.Models;

using System.Text.Json.Serialization;

/// <summary>Request body values for Vast offer search.</summary>
public sealed class VastSearchOffersRequest
{
  /// <summary>Maximum number of offers to return.</summary>
  public int Limit { get; set; } = 10;

  /// <summary>
  ///   Sort expression using the Vast CLI shorthand kept by the trainer profiles.
  ///   A trailing <c>-</c> sorts descending, a trailing <c>+</c> sorts ascending, and no suffix sorts ascending.
  /// </summary>
  public string Order { get; set; } = "dph_total";

  /// <summary>Vast instance type. The trainer uses on-demand rentals by default.</summary>
  public string Type { get; set; } = "on-demand";

  /// <summary>Filter fields keyed by Vast offer property name.</summary>
  public Dictionary<string, VastSearchFilter> Filters { get; } = [];
}

/// <summary>One Vast search filter with exactly one comparison operator normally set.</summary>
public sealed class VastSearchFilter
{
  /// <summary>Equal-to filter value.</summary>
  [JsonPropertyName("eq")]
  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  public object? EqualTo { get; set; }

  /// <summary>Greater-than-or-equal filter value.</summary>
  [JsonPropertyName("gte")]
  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  public object? GreaterThanOrEqualTo { get; set; }

  /// <summary>Less-than-or-equal filter value.</summary>
  [JsonPropertyName("lte")]
  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  public object? LessThanOrEqualTo { get; set; }

  /// <summary>Value-is-in filter values.</summary>
  [JsonPropertyName("in")]
  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  public IReadOnlyList<object>? In { get; set; }

  /// <summary>Value-is-not-in filter values.</summary>
  [JsonPropertyName("notin")]
  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  public IReadOnlyList<object>? NotIn { get; set; }
}
