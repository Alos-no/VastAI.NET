namespace VastAI.NET;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Configuration;
using Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Models;

/// <summary>Default HTTP implementation of <see cref="IVastApiClient" />.</summary>
public sealed class VastApiClient(
  HttpClient               httpClient,
  IOptions<VastAIOptions>  options,
  ILogger<VastApiClient>?  logger = null) : IVastApiClient
{
  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNamingPolicy        = JsonNamingPolicy.SnakeCaseLower,
    DefaultIgnoreCondition      = JsonIgnoreCondition.WhenWritingNull,
    PropertyNameCaseInsensitive = true
  };

  private readonly HttpClient _httpClient = httpClient;
  private readonly VastAIOptions _options = options.Value;
  private readonly ILogger<VastApiClient>? _logger = logger;

  /// <summary>
  ///   Creates a client from values known only at runtime, such as a browser-entered API key or an interactive
  ///   CLI prompt.
  /// </summary>
  /// <param name="apiKey">Vast API key sent as a bearer token on every request.</param>
  /// <param name="apiBaseUri">Optional Vast API base URI; defaults to Vast's public console endpoint.</param>
  /// <param name="httpClient">Optional HTTP client supplied by the host. Browser hosts should pass their configured client.</param>
  /// <param name="logger">Optional logger for failed HTTP responses.</param>
  /// <returns>A configured Vast REST client that does not depend on <c>IConfiguration</c> or DI.</returns>
  /// <exception cref="VastAIConfigurationException">Thrown when <paramref name="apiKey" /> is empty.</exception>
  public static VastApiClient Create(
    string                  apiKey,
    Uri?                    apiBaseUri = null,
    HttpClient?             httpClient = null,
    ILogger<VastApiClient>? logger = null)
  {
    if (string.IsNullOrWhiteSpace(apiKey))
      throw new VastAIConfigurationException("Vast API key is required.");

    return new VastApiClient(
      httpClient ?? new HttpClient(),
      Options.Create(new VastAIOptions
      {
        ApiKey     = apiKey,
        ApiBaseUri = apiBaseUri ?? new Uri("https://console.vast.ai/")
      }),
      logger);
  }

  /// <inheritdoc />
  public async Task<IReadOnlyList<VastOffer>> SearchOffersAsync(
    VastSearchOffersRequest request,
    CancellationToken       cancellationToken = default)
  {
    var body = CreateSearchBody(request);
    using var httpRequest = CreateRequest(HttpMethod.Post, "api/v0/bundles/", body);
    using var response    = await _httpClient.SendAsync(httpRequest, cancellationToken);
    var       json        = await ReadSuccessfulJsonAsync(response, cancellationToken);
    return VastJsonReader.ReadOffers(json);
  }

  /// <inheritdoc />
  public async Task<IReadOnlyList<VastInstance>> GetInstancesAsync(CancellationToken cancellationToken = default)
  {
    using var request  = CreateRequest(HttpMethod.Get, "api/v1/instances/");
    using var response = await _httpClient.SendAsync(request, cancellationToken);
    var       json     = await ReadSuccessfulJsonAsync(response, cancellationToken);
    return VastJsonReader.ReadInstances(json);
  }

  /// <inheritdoc />
  public async Task<VastInstance> GetInstanceAsync(string instanceId, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(instanceId))
      throw new ArgumentException("Instance id is required.", nameof(instanceId));

    using var request  = CreateRequest(HttpMethod.Get, $"api/v0/instances/{Uri.EscapeDataString(instanceId)}/");
    using var response = await _httpClient.SendAsync(request, cancellationToken);
    var       json     = await ReadSuccessfulJsonAsync(response, cancellationToken);
    return VastJsonReader.ReadInstance(json);
  }

  /// <inheritdoc />
  public async Task<VastCreateInstanceResult> CreateInstanceAsync(
    string                    offerId,
    VastCreateInstanceRequest request,
    CancellationToken         cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(offerId))
      throw new ArgumentException("Offer id is required.", nameof(offerId));

    using var httpRequest = CreateRequest(HttpMethod.Put, $"api/v0/asks/{Uri.EscapeDataString(offerId)}/", CreateInstanceBody(request));
    using var response    = await _httpClient.SendAsync(httpRequest, cancellationToken);
    var       json        = await ReadSuccessfulJsonAsync(response, cancellationToken);
    return new VastCreateInstanceResult(VastJsonReader.ReadNewContract(json));
  }

  /// <inheritdoc />
  public async Task DestroyInstanceAsync(string instanceId, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(instanceId))
      throw new ArgumentException("Instance id is required.", nameof(instanceId));

    using var request  = CreateRequest(HttpMethod.Delete, $"api/v0/instances/{Uri.EscapeDataString(instanceId)}/");
    using var response = await _httpClient.SendAsync(request, cancellationToken);
    await ReadSuccessfulJsonAsync(response, cancellationToken);
  }

  /// <summary>Creates an authenticated request with an optional JSON body.</summary>
  private HttpRequestMessage CreateRequest(HttpMethod method, string relativePath, object? body = null)
  {
    if (string.IsNullOrWhiteSpace(_options.ApiKey))
      throw new VastAIConfigurationException("Vast API key is required. Set VastAI:ApiKey or VAST_API_KEY before calling the API.");

    var requestUri = new Uri(_options.ApiBaseUri, relativePath);
    var request    = new HttpRequestMessage(method, requestUri);
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey.Trim());
    if (body is not null)
      request.Content = JsonContent.Create(body, options: JsonOptions);
    return request;
  }

  /// <summary>Converts the public search model into Vast's flat JSON body shape.</summary>
  private static Dictionary<string, object?> CreateSearchBody(VastSearchOffersRequest request)
  {
    var body = new Dictionary<string, object?>
    {
      ["limit"] = request.Limit,
      ["type"]  = request.Type,
      ["order"] = CreateOrder(request.Order)
    };

    foreach (var (name, filter) in request.Filters)
      body[name] = filter;

    return body;
  }

  /// <summary>Converts a compact sort expression into Vast's REST API list-based order shape.</summary>
  private static IReadOnlyList<IReadOnlyList<string>> CreateOrder(string order)
  {
    var trimmedOrder = string.IsNullOrWhiteSpace(order) ? "dph_total" : order.Trim();
    var direction    = trimmedOrder.EndsWith("-", StringComparison.Ordinal) ? "desc" : "asc";
    var fieldName    = trimmedOrder.TrimEnd('-', '+');
    return [[fieldName, direction]];
  }

  /// <summary>Converts the public create-instance model into Vast's REST body shape.</summary>
  private static Dictionary<string, object?> CreateInstanceBody(VastCreateInstanceRequest request)
  {
    var body = new Dictionary<string, object?>
    {
      ["disk"]           = request.DiskGb,
      ["label"]          = request.Label,
      ["runtype"]        = request.RuntimeType,
      ["env"]            = request.Environment.Count == 0 ? null : request.Environment,
      ["onstart"]        = string.IsNullOrWhiteSpace(request.OnStart) ? null : request.OnStart,
      ["cancel_unavail"] = request.CancelUnavailable
    };

    if (!string.IsNullOrWhiteSpace(request.TemplateHashId))
      body["template_hash_id"] = request.TemplateHashId;
    else if (!string.IsNullOrWhiteSpace(request.Image))
      body["image"] = request.Image;

    return body;
  }

  /// <summary>Reads response JSON or throws a Vast-specific exception with the server payload.</summary>
  private async Task<string> ReadSuccessfulJsonAsync(HttpResponseMessage response, CancellationToken cancellationToken)
  {
    var json = await response.Content.ReadAsStringAsync(cancellationToken);
    if (response.IsSuccessStatusCode)
      return json;

    _logger?.LogWarning("Vast API request failed with {StatusCode}: {Body}", (int)response.StatusCode, json);
    throw new VastAIOperationException($"Vast API returned {(int)response.StatusCode}: {json}");
  }
}
