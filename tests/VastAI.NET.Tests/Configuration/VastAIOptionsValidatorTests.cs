namespace VastAI.NET.Tests.Configuration;

using VastAI.NET.Configuration;

/// <summary>Tests validation rules for the Vast API client options.</summary>
public sealed class VastAIOptionsValidatorTests
{
  private readonly VastAIOptionsValidator _validator = new();

  /// <summary>Validation fails when no API key is configured.</summary>
  [Fact]
  public void Validate_MissingApiKey_Fails()
  {
    var result = _validator.Validate(null, new VastAIOptions { ApiKey = "" });

    Assert.True(result.Failed);
    Assert.Contains("ApiKey", string.Join(Environment.NewLine, result.Failures));
  }

  /// <summary>Validation succeeds when API key and base URI are configured.</summary>
  [Fact]
  public void Validate_ConfiguredApiKeyAndBaseUri_Succeeds()
  {
    var result = _validator.Validate(null, new VastAIOptions
    {
      ApiKey     = "test-key",
      ApiBaseUri = new Uri("https://console.vast.ai/")
    });

    Assert.False(result.Failed);
  }
}
