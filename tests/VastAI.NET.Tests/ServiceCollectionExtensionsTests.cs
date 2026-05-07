namespace VastAI.NET.Tests;

using Microsoft.Extensions.DependencyInjection;

/// <summary>Tests dependency injection registration for the Vast REST client.</summary>
public sealed class ServiceCollectionExtensionsTests
{
  /// <summary>
  ///   Resolves the typed Vast client from DI so application hosts can reuse one registration path instead of
  ///   constructing their own HTTP clients.
  /// </summary>
  [Fact]
  public void AddVastAI_RegistersTypedClient()
  {
    var services = new ServiceCollection();
    services.AddVastAI(options => options.ApiKey = "test-key");

    using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });

    Assert.IsType<VastApiClient>(provider.GetRequiredService<IVastApiClient>());
  }
}
