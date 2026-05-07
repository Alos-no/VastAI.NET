namespace VastAI.NET.Configuration;

/// <summary>Configuration used by <see cref="VastApiClient" /> when it calls the Vast.ai REST API.</summary>
public sealed class VastAIOptions
{
  /// <summary>Configuration section name used by the service collection extensions.</summary>
  public const string SectionName = "VastAI";

  /// <summary>Vast API key sent as a bearer token on every authenticated request.</summary>
  public string? ApiKey { get; set; }

  /// <summary>Base URI for Vast's console API. Defaults to the public production endpoint.</summary>
  public Uri ApiBaseUri { get; set; } = new("https://console.vast.ai/");

  /// <summary>HTTP resilience settings used when registering the typed client through dependency injection.</summary>
  public ResilienceOptions Resilience { get; set; } = new();
}
