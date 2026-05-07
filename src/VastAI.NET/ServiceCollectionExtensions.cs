namespace VastAI.NET;

using Configuration;
using Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

/// <summary>Registers the Vast.ai REST client and its configuration.</summary>
public static class ServiceCollectionExtensions
{
  /// <summary>Registers <see cref="IVastApiClient" /> using the <c>VastAI</c> configuration section.</summary>
  /// <param name="services">Service collection to configure.</param>
  /// <param name="configuration">Application configuration containing a <c>VastAI</c> section.</param>
  /// <returns>The same service collection for chaining.</returns>
  public static IServiceCollection AddVastAI(this IServiceCollection services, IConfiguration configuration) =>
    services.AddVastAI(options => configuration.GetSection(VastAIOptions.SectionName).Bind(options));

  /// <summary>Registers <see cref="IVastApiClient" /> using programmatic options.</summary>
  /// <param name="services">Service collection to configure.</param>
  /// <param name="configureOptions">Callback that supplies the Vast API key, base URI, and resilience settings.</param>
  /// <returns>The same service collection for chaining.</returns>
  public static IServiceCollection AddVastAI(this IServiceCollection services, Action<VastAIOptions> configureOptions)
  {
    services.Configure(configureOptions);
    services.TryAddSingleton<IValidateOptions<VastAIOptions>, VastAIOptionsValidator>();
    services.AddOptions<VastAIOptions>().ValidateOnStart();

    services.AddVastAIHttpClient()
            .ConfigureHttpClient((provider, client) =>
            {
              var options = provider.GetRequiredService<IOptions<VastAIOptions>>().Value;
              client.BaseAddress = options.ApiBaseUri;
            });

    services.TryAddTransient<IVastApiClient, VastApiClient>();
    return services;
  }
}
