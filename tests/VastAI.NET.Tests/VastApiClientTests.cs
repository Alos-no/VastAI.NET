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
                                           "ask_contract_id": 456,
                                           "bundle_id": 789,
                                           "bw_nvlink": 0,
                                           "gpu_name": "RTX 4090",
                                           "num_gpus": 1,
                                           "dph_total": 0.25,
                                           "dph_base": 0.20,
                                           "cuda_max_good": 12.8,
                                           "reliability": 0.999,
                                           "expected_reliability": 0.997,
                                           "disk_space": 500,
                                           "gpu_ram": 24576,
                                           "gpu_total_ram": 24576,
                                           "gpu_mem_bw": 1008.2,
                                           "gpu_lanes": 16,
                                           "gpu_max_power": 450,
                                           "cpu_name": "AMD EPYC 9654",
                                           "cpu_arch": "amd64",
                                           "cpu_ram": 131072,
                                           "cpu_cores": 32,
                                           "cpu_cores_effective": 16,
                                           "cpu_ghz": 2.4,
                                           "disk_name": "NVMe SSD",
                                           "disk_bw": 6400,
                                           "direct_port_count": 128,
                                           "inet_up": 900,
                                           "inet_down": 800,
                                           "inet_up_cost": 0.00049,
                                           "inet_down_cost": 0.00024,
                                           "internet_up_cost_per_tb": 0.5,
                                           "internet_down_cost_per_tb": 0.25,
                                           "compute_cap": 890,
                                           "geolocation": "US",
                                           "driver_version": "570.86.15",
                                           "public_ipaddr": "203.0.113.42",
                                           "dlperf": 198.7,
                                           "dlperf_per_dphtotal": 794.8,
                                           "total_flops": 82.6,
                                           "duration": 86400,
                                           "machine_id": 1234,
                                           "host_id": 5678,
                                           "verification": "verified",
                                           "rentable": true,
                                           "rented": false,
                                           "search": {
                                             "gpuCostPerHour": 0.20,
                                             "diskHour": 0.01,
                                             "totalHour": 0.25,
                                             "discountTotalHour": 0.02,
                                             "discountedTotalPerHour": 0.23
                                           }
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
    Assert.Equal("456", offers[0].AskContractId);
    Assert.Equal("789", offers[0].BundleId);
    Assert.Equal(1, offers[0].NumGpus);
    Assert.Equal(24, offers[0].GpuRamGb);
    Assert.Equal(24, offers[0].GpuTotalRamGb);
    Assert.Equal(1008.2, offers[0].GpuMemBwGbPerSecond);
    Assert.Equal(16, offers[0].GpuLanes);
    Assert.Equal(450, offers[0].GpuMaxPowerWatts);
    Assert.Equal("AMD EPYC 9654", offers[0].CpuName);
    Assert.Equal("amd64", offers[0].CpuArch);
    Assert.Equal(128, offers[0].CpuRamGb);
    Assert.Equal(32, offers[0].CpuCores);
    Assert.Equal(16, offers[0].CpuCoresEffective);
    Assert.Equal(2.4, offers[0].CpuGhz);
    Assert.Equal("NVMe SSD", offers[0].DiskName);
    Assert.Equal(6400, offers[0].DiskBwMbPerSecond);
    Assert.Equal(128, offers[0].DirectPortCount);
    Assert.Equal("570.86.15", offers[0].DriverVersion);
    Assert.Equal("203.0.113.42", offers[0].PublicIpAddress);
    Assert.Equal(198.7, offers[0].DlPerf);
    Assert.Equal(794.8, offers[0].DlPerfPerDollarHour);
    Assert.Equal(82.6, offers[0].TotalFlops);
    Assert.Equal(86400, offers[0].DurationSeconds);
    Assert.Equal("1234", offers[0].MachineId);
    Assert.Equal("5678", offers[0].HostId);
    Assert.True(offers[0].Rentable);
    Assert.False(offers[0].Rented);
    Assert.Equal("verified", offers[0].Verification);
    Assert.Equal(0.20, offers[0].SearchPricing?.GpuCostPerHour);
    Assert.Equal(0.25, offers[0].SearchPricing?.TotalHour);
    Assert.Equal(0.5, offers[0].InternetUpCostPerTb);
    Assert.True(offers[0].Raw.TryGetProperty("cpu_name", out var cpuName));
    Assert.Equal("AMD EPYC 9654", cpuName?.GetValue<string>());
    Assert.True(offers[0].Raw.TryGetProperty("gpu_ram", out var gpuRam));
    Assert.Equal(24576, gpuRam?.GetValue<double>());
    Assert.True(offers[0].Raw.TryGetProperty("disk_name", out var storageName));
    Assert.Equal("NVMe SSD", storageName?.GetValue<string>());
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

  /// <summary>Create instance must fail with a readable operation error when Vast returns success without a contract id.</summary>
  [Fact]
  public async Task CreateInstanceAsync_WhenSuccessResponseHasNoContractId_ThrowsReadableOperationError()
  {
    var handler = new CaptureHandler("");
    var client  = CreateClient(handler);

    var exception = await Assert.ThrowsAsync<Exceptions.VastAIOperationException>(() =>
      client.CreateInstanceAsync(
        "12345",
        new VastCreateInstanceRequest { DiskGb = 85, RuntimeType = "ssh" },
        TestContext.Current.CancellationToken));

    Assert.Contains("create-instance response did not include a new contract id", exception.Message);
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

  /// <summary>Show instances preserves browser recovery metadata and the full raw instance object.</summary>
  [Fact]
  public async Task GetInstancesAsync_PreservesExtraEnvPortsPublicIpAndRawData()
  {
    var handler = new CaptureHandler("""
                                     {
                                       "instances": [
                                         {
                                           "id": 42,
                                           "actual_status": "running",
                                           "label": "vlt1-run123-demo",
                                           "gpu_name": "RTX 4060 Ti",
                                           "dph_total": 0.116,
                                           "public_ipaddr": "203.0.113.10",
                                           "extra_env": [
                                             ["VAST_LFS_TOKEN", "worker-token"],
                                             ["VAST_LFS_RUN_ID", "run123"]
                                           ],
                                           "ports": {
                                             "8088/tcp": [
                                               { "HostIp": "0.0.0.0", "HostPort": "18088" }
                                             ]
                                           },
                                           "future_vast_field": { "nested": true }
                                         }
                                       ]
                                     }
                                     """);
    var client = CreateClient(handler);

    var instances = await client.GetInstancesAsync(TestContext.Current.CancellationToken);

    var instance = Assert.Single(instances);
    Assert.Equal("203.0.113.10", instance.PublicIpAddress);
    Assert.Equal("worker-token", instance.ExtraEnvironment["VAST_LFS_TOKEN"]);
    Assert.True(instance.TryGetExtraEnvironmentValue("VAST_LFS_RUN_ID", out var runId));
    Assert.Equal("run123", runId);
    Assert.True(instance.TryGetPublicUriForContainerPort(8088, out var workerUri));
    Assert.Equal("http://203.0.113.10:18088/", workerUri.ToString());
    Assert.True(instance.Raw.TryGetProperty("future_vast_field", out var futureField));
    Assert.True(futureField?["nested"]?.GetValue<bool>());
    Assert.True(instance.Raw.TryGetProperty("extra_env", out _));
  }

  /// <summary>Show instances accepts Vast port mappings when they arrive as an array of objects.</summary>
  [Fact]
  public async Task GetInstancesAsync_ParsesArrayPortMappings()
  {
    var handler = new CaptureHandler("""
                                     {
                                       "instances": [
                                         {
                                           "id": 42,
                                           "actual_status": "running",
                                           "public_ipaddr": "203.0.113.10",
                                           "ports": [
                                             { "container_port": 8088, "host_port": 18088, "protocol": "tcp", "host": "198.51.100.25" }
                                           ]
                                         }
                                       ]
                                     }
                                     """);
    var client = CreateClient(handler);

    var instance = Assert.Single(await client.GetInstancesAsync(TestContext.Current.CancellationToken));

    Assert.True(instance.TryGetPublicUriForContainerPort(8088, out var workerUri));
    Assert.Equal("http://198.51.100.25:18088/", workerUri.ToString());
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
