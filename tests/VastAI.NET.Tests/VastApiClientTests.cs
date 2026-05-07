namespace VastAI.NET.Tests;

using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Models;
using VastAI.NET.Configuration;

/// <summary>Tests the HTTP contract used by <see cref="VastApiClient" /> without calling the live Vast API.</summary>
public sealed class VastApiClientTests
{
  /// <summary>Search sends bearer auth, the official bundles endpoint, and Vast filter objects.</summary>
  [Fact]
  public async Task SearchOffersAsync_SendsAuthenticatedBundlesRequestAndParsesOffers()
  {
    var handler = new CaptureHandler("""
                                     {
                                       "offers": [
                                         {
                                           "id": 123,
                                           "gpu_name": "RTX 4090",
                                           "dph_total": 0.25,
                                           "cuda_max_good": 12.8,
                                           "reliability": 0.999,
                                           "disk_space": 500,
                                           "inet_up": 900,
                                           "inet_down": 800,
                                           "internet_up_cost_per_tb": 0.5,
                                           "internet_down_cost_per_tb": 0.25,
                                           "compute_cap": 890,
                                           "geolocation": "US"
                                         }
                                       ]
                                     }
                                     """);
    var client = CreateClient(handler);
    var search = new VastSearchOffersRequest { Limit = 5, Order = "dph_total" };
    search.Filters["verified"] = new VastSearchFilter { EqualTo = true };
    search.Filters["gpu_name"] = new VastSearchFilter { In      = ["RTX_4090", "RTX_5090"] };

    var offers = await client.SearchOffersAsync(search, TestContext.Current.CancellationToken);

    Assert.Equal(HttpMethod.Post, handler.Request!.Method);
    Assert.Equal("https://console.vast.ai/api/v0/bundles/", handler.Request.RequestUri!.ToString());
    Assert.Equal("Bearer", handler.Request.Headers.Authorization!.Scheme);
    Assert.Equal("test-key", handler.Request.Headers.Authorization.Parameter);
    Assert.Contains("\"verified\":{\"eq\":true}", handler.Body);
    Assert.Contains("\"gpu_name\":{\"in\":[\"RTX_4090\",\"RTX_5090\"]}", handler.Body);
    Assert.Contains("\"order\":[[\"dph_total\",\"asc\"]]", handler.Body);
    Assert.Single(offers);
    Assert.Equal("123", offers[0].Id);
    Assert.Equal(0.5, offers[0].InternetUpCostPerTb);
  }

  /// <summary>Search converts a descending compact sort expression to the REST API order-list shape.</summary>
  [Fact]
  public async Task SearchOffersAsync_WithDescendingOrderSuffix_SendsRestOrderList()
  {
    var handler = new CaptureHandler("""{"offers":[]}""");
    var client  = CreateClient(handler);

    await client.SearchOffersAsync(new VastSearchOffersRequest { Order = "dlperf_usd-" }, TestContext.Current.CancellationToken);

    Assert.Contains("\"order\":[[\"dlperf_usd\",\"desc\"]]", handler.Body);
  }

  /// <summary>Create instance sends Vast's create endpoint and returns the new contract id.</summary>
  [Fact]
  public async Task CreateInstanceAsync_SendsCreateRequestAndParsesContractId()
  {
    var handler = new CaptureHandler("""{"new_contract":987654}""");
    var client  = CreateClient(handler);

    var result = await client.CreateInstanceAsync(
      "12345",
      new VastCreateInstanceRequest
      {
        DiskGb            = 85,
        Label             = "lfs-smoke",
        Image             = "vastai/base-image:@vastai-automatic-tag",
        RuntimeType       = "ssh",
        Environment       = { ["-p 8088:8088"] = "1" },
        CancelUnavailable = true
      },
      TestContext.Current.CancellationToken);

    Assert.Equal(HttpMethod.Put, handler.Request!.Method);
    Assert.Equal("https://console.vast.ai/api/v0/asks/12345/", handler.Request.RequestUri!.ToString());
    Assert.Contains("\"disk\":85", handler.Body);
    Assert.Contains("\"runtype\":\"ssh\"", handler.Body);
    Assert.Contains("\"env\":{\"-p 8088:8088\":\"1\"}", handler.Body);
    Assert.Contains("\"image\":\"vastai/base-image:@vastai-automatic-tag\"", handler.Body);
    Assert.Equal("987654", result.InstanceId);
  }

  /// <summary>Show instances parses wrapped instance arrays and preserves SSH metadata when Vast returns it.</summary>
  [Fact]
  public async Task GetInstancesAsync_ParsesInstancesAndSshTargetFields()
  {
    var handler = new CaptureHandler("""
                                     {
                                       "instances": [
                                         {
                                           "id": 42,
                                           "actual_status": "running",
                                           "label": "lfs-job",
                                           "gpu_name": "RTX 4060 Ti",
                                           "dph_total": 0.116,
                                           "age_hours": 1.5,
                                           "ssh_host": "203.0.113.10",
                                           "ssh_port": 40123,
                                           "ssh_user": "root"
                                         }
                                       ]
                                     }
                                     """);
    var client = CreateClient(handler);

    var instances = await client.GetInstancesAsync(TestContext.Current.CancellationToken);

    Assert.Equal(HttpMethod.Get, handler.Request!.Method);
    Assert.Equal("https://console.vast.ai/api/v1/instances/", handler.Request.RequestUri!.ToString());
    Assert.Single(instances);
    Assert.True(instances[0].IsActive);
    Assert.True(instances[0].HasSshTarget);
    Assert.Equal("203.0.113.10", instances[0].SshHost);
    Assert.Equal(40123, instances[0].SshPort);
  }

  /// <summary>Show instance accepts Vast's single-object instances wrapper returned by the live API.</summary>
  [Fact]
  public async Task GetInstanceAsync_ParsesSingleInstanceObjectWrapper()
  {
    var handler = new CaptureHandler("""
                                     {
                                       "instances": {
                                         "id": 42,
                                         "actual_status": "running",
                                         "label": "lfs-job",
                                         "gpu_name": "RTX 4060 Ti",
                                         "dph_total": 0.116
                                       }
                                     }
                                     """);
    var client = CreateClient(handler);

    var instance = await client.GetInstanceAsync("42", TestContext.Current.CancellationToken);

    Assert.Equal(HttpMethod.Get, handler.Request!.Method);
    Assert.Equal("https://console.vast.ai/api/v0/instances/42/", handler.Request.RequestUri!.ToString());
    Assert.Equal("42", instance.Id);
    Assert.Equal("lfs-job", instance.Label);
  }

  /// <summary>Destroy instance uses HTTP DELETE and treats a successful empty body as success.</summary>
  [Fact]
  public async Task DestroyInstanceAsync_SendsDeleteRequest()
  {
    var handler = new CaptureHandler("""{"success":true}""");
    var client  = CreateClient(handler);

    await client.DestroyInstanceAsync("12345", TestContext.Current.CancellationToken);

    Assert.Equal(HttpMethod.Delete, handler.Request!.Method);
    Assert.Equal("https://console.vast.ai/api/v0/instances/12345/", handler.Request.RequestUri!.ToString());
  }

  /// <summary>Destroy retries a transient Vast rate-limit response because cleanup must not leak paid instances.</summary>
  [Fact]
  public async Task DestroyInstanceAsync_WhenVastRateLimits_RetriesAndSucceeds()
  {
    var handler = new SequenceHandler(
      new HttpResponseMessage((HttpStatusCode)429) { Content = new StringContent("Too Many Requests") },
      new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("""{"success":true}""") });
    var client = CreateClient(handler);

    await client.DestroyInstanceAsync("12345", TestContext.Current.CancellationToken);

    Assert.Equal(2, handler.Requests.Count);
    Assert.All(handler.Requests, request => Assert.Equal(HttpMethod.Delete, request.Method));
  }

  /// <summary>Create-instance does not retry 429 responses because retrying a rental request can create duplicate instances.</summary>
  [Fact]
  public async Task CreateInstanceAsync_WhenVastRateLimits_DoesNotRetry()
  {
    var handler = new SequenceHandler(
      new HttpResponseMessage((HttpStatusCode)429) { Content = new StringContent("Too Many Requests") },
      new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("""{"new_contract":123}""") });
    var client = CreateClient(handler);

    await Assert.ThrowsAsync<Exceptions.VastAIOperationException>(() => client.CreateInstanceAsync(
                                                                    "12345",
                                                                    new VastCreateInstanceRequest { DiskGb = 20 },
                                                                    TestContext.Current.CancellationToken));

    Assert.Single(handler.Requests);
    Assert.Equal(HttpMethod.Put, handler.Requests[0].Method);
  }

  /// <summary>Calls fail before HTTP when no API key was configured, avoiding an invalid bearer header.</summary>
  [Fact]
  public async Task GetInstancesAsync_WithoutApiKey_ThrowsConfigurationErrorBeforeSendingRequest()
  {
    var handler = new CaptureHandler("""{"instances":[]}""");
    var client = new VastApiClient(
      new HttpClient(handler),
      Options.Create(new VastAIOptions { ApiKey = "" }),
      NullLogger<VastApiClient>.Instance);

    await Assert.ThrowsAsync<Exceptions.VastAIConfigurationException>(() => client.GetInstancesAsync(
                                                                        TestContext.Current.CancellationToken));

    Assert.Null(handler.Request);
  }

  /// <summary>Creates a client from a runtime API key without requiring dependency injection or IConfiguration.</summary>
  [Fact]
  public async Task Create_WithRuntimeApiKey_SendsAuthenticatedRequests()
  {
    var       handler    = new CaptureHandler("""{"instances":[]}""");
    using var httpClient = new HttpClient(handler);
    var       client     = VastApiClient.Create(" runtime-key " + Environment.NewLine, httpClient: httpClient);

    await client.GetInstancesAsync(TestContext.Current.CancellationToken);

    Assert.Equal("Bearer", handler.Request!.Headers.Authorization!.Scheme);
    Assert.Equal("runtime-key", handler.Request.Headers.Authorization.Parameter);
  }

  /// <summary>Rejects empty runtime API keys at factory time so callers fail before issuing live HTTP requests.</summary>
  [Fact]
  public void Create_WithMissingRuntimeApiKey_ThrowsConfigurationException()
  {
    var exception = Assert.Throws<Exceptions.VastAIConfigurationException>(() => VastApiClient.Create(""));

    Assert.Contains("Vast API key is required", exception.Message);
  }

  /// <summary>Failed Vast responses must not copy the configured bearer token into exceptions or logs.</summary>
  [Fact]
  public async Task GetInstancesAsync_WhenRequestFails_DoesNotLeakApiKeyInExceptionOrLogs()
  {
    const string apiKey = "live-secret-value-that-must-not-appear";
    var handler = new SequenceHandler(
      new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = new StringContent("""{"error":"unauthorized"}""") });
    var logger = new CapturingLogger<VastApiClient>();
    var client = new VastApiClient(
      new HttpClient(handler),
      Options.Create(new VastAIOptions { ApiKey = apiKey, ApiBaseUri = new Uri("https://console.vast.ai/") }),
      logger);

    var exception = await Assert.ThrowsAsync<Exceptions.VastAIOperationException>(() => client.GetInstancesAsync(
                                                                                    TestContext.Current.CancellationToken));

    Assert.DoesNotContain(apiKey, exception.ToString(), StringComparison.Ordinal);
    Assert.DoesNotContain(apiKey, string.Join(Environment.NewLine, logger.Messages), StringComparison.Ordinal);
  }

  /// <summary>Creates a client with a fake handler and deterministic options.</summary>
  private static VastApiClient CreateClient(HttpMessageHandler handler)
  {
    var httpClient = new HttpClient(handler);
    var options = Options.Create(new VastAIOptions
    {
      ApiKey     = "test-key",
      ApiBaseUri = new Uri("https://console.vast.ai/")
    });
    return new VastApiClient(httpClient, options, NullLogger<VastApiClient>.Instance);
  }

  /// <summary>Captures the outgoing request while returning a configured JSON payload.</summary>
  private sealed class CaptureHandler(string responseJson) : HttpMessageHandler
  {
    public HttpRequestMessage? Request { get; private set; }
    public string              Body    { get; private set; } = "";

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
      Request = request;
      if (request.Content is not null)
        Body = await request.Content.ReadAsStringAsync(cancellationToken);

      return new HttpResponseMessage(HttpStatusCode.OK)
      {
        Content = new StringContent(responseJson)
      };
    }
  }

  /// <summary>Returns a deterministic response sequence while recording every request sent by the client.</summary>
  private sealed class SequenceHandler(params HttpResponseMessage[] responses) : HttpMessageHandler
  {
    private readonly Queue<HttpResponseMessage> _responses = new(responses);

    public List<HttpRequestMessage> Requests { get; } = [];

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
      Requests.Add(request);
      return Task.FromResult(_responses.Count == 0
        ? new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("""{"success":true}""") }
        : _responses.Dequeue());
    }
  }

  /// <summary>Captures rendered log messages without writing them to test output.</summary>
  private sealed class CapturingLogger<T> : ILogger<T>
  {
    public List<string> Messages { get; } = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
      LogLevel                        logLevel,
      EventId                         eventId,
      TState                          state,
      Exception?                      exception,
      Func<TState, Exception?, string> formatter)
    {
      Messages.Add(formatter(state, exception));
      if (exception is not null)
        Messages.Add(exception.ToString());
    }
  }

  /// <summary>Reusable no-op logging scope.</summary>
  private sealed class NullScope : IDisposable
  {
    public static readonly NullScope Instance = new();

    public void Dispose()
    {
    }
  }
}
