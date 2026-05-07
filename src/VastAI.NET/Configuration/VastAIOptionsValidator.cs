namespace VastAI.NET.Configuration;

using Microsoft.Extensions.Options;

/// <summary>Validates required Vast API client configuration before the first paid or read-only API call runs.</summary>
public sealed class VastAIOptionsValidator : IValidateOptions<VastAIOptions>
{
  /// <summary>Validates API key and base URI settings.</summary>
  /// <param name="name">Named options instance being validated.</param>
  /// <param name="options">Vast API options to validate.</param>
  /// <returns>Validation success or a list of actionable configuration errors.</returns>
  public ValidateOptionsResult Validate(string? name, VastAIOptions options)
  {
    var errors = new List<string>();
    if (string.IsNullOrWhiteSpace(options.ApiKey))
      errors.Add($"{VastAIOptions.SectionName}:ApiKey is required.");

    if (!options.ApiBaseUri.IsAbsoluteUri)
      errors.Add($"{VastAIOptions.SectionName}:ApiBaseUri must be an absolute URI.");

    return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
  }
}
