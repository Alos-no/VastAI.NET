namespace VastAI.NET.Tests;

using Microsoft.Extensions.Configuration;
using Models;

/// <summary>Live tests for the public Vast REST API client.</summary>
public sealed class LiveVastApiTests
{
  /// <summary>Verifies that a configured API key can read the account's current instances without using the Vast CLI.</summary>
  [Fact(Explicit = true)]
  public async Task GetInstancesAsync_WithLiveApiKey_ReturnsAccountInstanceList()
  {
    var client    = CreateLiveClient();
    var instances = await client.GetInstancesAsync(TestContext.Current.CancellationToken);

    Assert.NotNull(instances);
    Assert.All(instances, instance => Assert.False(string.IsNullOrWhiteSpace(instance.Id)));
  }

  /// <summary>
  ///   Verifies that the library can query the live offer marketplace with cheap, read-only filters.
  ///   The assertion allows zero offers because market availability changes constantly.
  /// </summary>
  [Fact(Explicit = true)]
  public async Task SearchOffersAsync_WithLiveApiKey_ReturnsMarketplaceResponse()
  {
    var client = CreateLiveClient();
    var search = new VastSearchOffersRequest
    {
      Limit = 3,
      Order = "dph_total"
    };
    search.Filters["rentable"]       = new VastSearchFilter { EqualTo           = true };
    search.Filters["verified"]       = new VastSearchFilter { EqualTo           = true };
    search.Filters["dph_total"]      = new VastSearchFilter { LessThanOrEqualTo = 0.20 };
    search.Filters["inet_up_cost"]   = new VastSearchFilter { LessThanOrEqualTo = 1.0 / 1024.0 };
    search.Filters["inet_down_cost"] = new VastSearchFilter { LessThanOrEqualTo = 1.0 / 1024.0 };

    var offers = await client.SearchOffersAsync(search, TestContext.Current.CancellationToken);

    Assert.NotNull(offers);
    Assert.All(offers, offer =>
    {
      Assert.False(string.IsNullOrWhiteSpace(offer.Id));
      Assert.True(offer.Price <= 0.20, $"Offer {offer.Id} costs {offer.Price} per hour.");
      Assert.True(offer.InternetUpCostPerTb <= 1.0, $"Offer {offer.Id} upload bandwidth costs {offer.InternetUpCostPerTb} per TB.");
      Assert.True(offer.InternetDownCostPerTb <= 1.0, $"Offer {offer.Id} download bandwidth costs {offer.InternetDownCostPerTb} per TB.");
    });
  }

  /// <summary>Verifies create, inspect, destroy, and post-destroy listing against a real cheap Vast rental.</summary>
  [Fact(Explicit = true)]
  public async Task CreateGetInstanceDestroyAsync_WithCheapLiveOffer_RentsAndDestroysInstance()
  {
    var client = CreateLiveClient();
    await RequireNoActiveInstancesAsync(client);

    const double maxPricePerHour = 0.20;
    var          offer           = await FindCheapRentalOfferAsync(client, maxPricePerHour);
    var          label           = "vastai-net-live-" + Guid.NewGuid().ToString("N");
    var          instanceId      = "";

    try
    {
      var createResult = await client.CreateInstanceAsync(
        offer.Id,
        new VastCreateInstanceRequest
        {
          DiskGb            = 20,
          Label             = label,
          Image             = "vastai/base-image:@vastai-automatic-tag",
          RuntimeType       = "ssh",
          Environment       = { ["-p 8080:8080"] = "1", ["DATA_DIRECTORY"] = "/workspace/" },
          OnStart           = "echo vastai-net-live-test",
          CancelUnavailable = true
        },
        TestContext.Current.CancellationToken);

      instanceId = createResult.InstanceId;
      Assert.False(string.IsNullOrWhiteSpace(instanceId));

      var instance = await WaitForInstanceAsync(client, instanceId, label);
      Assert.False(string.IsNullOrWhiteSpace(instance.Id));
      Assert.True(
        instance.Id == instanceId || instance.Label == label,
        $"Expected created instance id {instanceId} or label {label}; got id {instance.Id}, label {instance.Label}.");
    }
    finally
    {
      await DestroyCreatedInstanceAsync(client, instanceId, label);
    }

    await AssertInstanceDestroyedAsync(client, instanceId, label);
  }

  /// <summary>Creates the live client from test-only secrets or an explicit environment variable.</summary>
  private static VastApiClient CreateLiveClient()
  {
    var apiKey = ResolveApiKey();
    if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "test-api-key")
      throw new InvalidOperationException("Live Vast tests require VastAI:ApiKey in user-secrets or VAST_API_KEY in the environment.");

    return VastApiClient.Create(apiKey);
  }

  /// <summary>Rejects paid live tests when unrelated active rentals are already present.</summary>
  private static async Task RequireNoActiveInstancesAsync(IVastApiClient client)
  {
    var activeInstances = (await client.GetInstancesAsync(TestContext.Current.CancellationToken)).Where(instance => instance.IsActive)
      .ToList();
    Assert.True(
      activeInstances.Count == 0,
      "Refusing live rental test while active Vast instances exist.");
  }

  /// <summary>Finds a currently rentable offer that is cheap enough for a paid API smoke test.</summary>
  private static async Task<VastOffer> FindCheapRentalOfferAsync(IVastApiClient client, double maxPricePerHour)
  {
    var search = new VastSearchOffersRequest
    {
      Limit = 20,
      Order = "dph_total"
    };
    search.Filters["rentable"]          = new VastSearchFilter { EqualTo              = true };
    search.Filters["rented"]            = new VastSearchFilter { EqualTo              = false };
    search.Filters["verified"]          = new VastSearchFilter { EqualTo              = true };
    search.Filters["dph_total"]         = new VastSearchFilter { LessThanOrEqualTo    = maxPricePerHour };
    search.Filters["disk_space"]        = new VastSearchFilter { GreaterThanOrEqualTo = 20 };
    search.Filters["direct_port_count"] = new VastSearchFilter { GreaterThanOrEqualTo = 1 };
    search.Filters["inet_up_cost"]      = new VastSearchFilter { LessThanOrEqualTo    = 1.0 / 1024.0 };
    search.Filters["inet_down_cost"]    = new VastSearchFilter { LessThanOrEqualTo    = 1.0 / 1024.0 };

    var offers = await client.SearchOffersAsync(search, TestContext.Current.CancellationToken);
    var offer = offers.FirstOrDefault(offer =>
                                        offer.Price <= maxPricePerHour &&
                                        offer.InternetUpCostPerTb <= 1.0 &&
                                        offer.InternetDownCostPerTb <= 1.0);

    Assert.NotNull(offer);
    return offer;
  }

  /// <summary>Polls until Vast exposes the created instance through the single-instance or list endpoint.</summary>
  private static async Task<VastInstance> WaitForInstanceAsync(IVastApiClient client, string instanceId, string label)
  {
    for (var attempt = 0; attempt < 30; attempt++)
    {
      var instance = await client.GetInstanceAsync(instanceId, TestContext.Current.CancellationToken);
      if (!string.IsNullOrWhiteSpace(instance.Id) || string.Equals(instance.Label, label, StringComparison.Ordinal))
        return instance;

      var listedInstance = (await client.GetInstancesAsync(TestContext.Current.CancellationToken))
        .FirstOrDefault(instance => instance.Id == instanceId || string.Equals(instance.Label, label, StringComparison.Ordinal));
      if (listedInstance is not null)
        return listedInstance;

      await Task.Delay(TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken);
    }

    throw new TimeoutException($"Created Vast instance {instanceId} with label {label} was not visible after 60 seconds.");
  }

  /// <summary>Destroys the created instance and any same-label duplicate returned by Vast after a partial failure.</summary>
  private static async Task DestroyCreatedInstanceAsync(IVastApiClient client, string instanceId, string label)
  {
    var ids = new HashSet<string>(StringComparer.Ordinal);
    if (!string.IsNullOrWhiteSpace(instanceId))
      ids.Add(instanceId);

    var matchingInstances = (await client.GetInstancesAsync(TestContext.Current.CancellationToken))
                            .Where(instance => string.Equals(instance.Label, label, StringComparison.Ordinal))
                            .Select(instance => instance.Id);
    foreach (var id in matchingInstances)
      ids.Add(id);

    foreach (var id in ids)
      await client.DestroyInstanceAsync(id, TestContext.Current.CancellationToken);
  }

  /// <summary>Verifies Vast no longer lists the instance created by the paid live test.</summary>
  private static async Task AssertInstanceDestroyedAsync(IVastApiClient client, string instanceId, string label)
  {
    for (var attempt = 0; attempt < 30; attempt++)
    {
      var instances = await client.GetInstancesAsync(TestContext.Current.CancellationToken);
      if (instances.All(instance => instance.Id != instanceId && !string.Equals(instance.Label, label, StringComparison.Ordinal)))
        return;

      await Task.Delay(TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken);
    }

    Assert.Fail($"Vast still lists live-test instance {instanceId} with label {label} after destroy.");
  }

  /// <summary>Reads the API key without coupling live tests to the application's configuration model.</summary>
  private static string ResolveApiKey()
  {
    var environmentApiKey = Environment.GetEnvironmentVariable("VAST_API_KEY");
    if (!string.IsNullOrWhiteSpace(environmentApiKey))
      return environmentApiKey;

    var configuration = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json", true)
                        .AddUserSecrets<LiveVastApiTests>(true)
                        .Build();

    return configuration["VastAI:ApiKey"] ??
      configuration["Vast:ApiKey"] ??
      configuration["VAST_API_KEY"] ??
      "";
  }
}
