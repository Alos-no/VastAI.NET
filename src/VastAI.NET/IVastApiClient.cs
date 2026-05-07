namespace VastAI.NET;

using Models;

/// <summary>Typed client for the Vast.ai REST endpoints needed to search, rent, inspect, and destroy instances.</summary>
public interface IVastApiClient
{
  /// <summary>Searches Vast marketplace offers using a raw Vast query string and ordering expression.</summary>
  /// <param name="request">Search query, result limit, and order.</param>
  /// <param name="cancellationToken">Cancels the HTTP request.</param>
  /// <returns>Normalized offers returned by Vast.</returns>
  Task<IReadOnlyList<VastOffer>> SearchOffersAsync(VastSearchOffersRequest request, CancellationToken cancellationToken = default);

  /// <summary>Lists Vast instances visible to the authenticated account.</summary>
  /// <param name="cancellationToken">Cancels the HTTP request.</param>
  /// <returns>Normalized account instances.</returns>
  Task<IReadOnlyList<VastInstance>> GetInstancesAsync(CancellationToken cancellationToken = default);

  /// <summary>Loads one Vast instance by id.</summary>
  /// <param name="instanceId">Vast instance or contract id.</param>
  /// <param name="cancellationToken">Cancels the HTTP request.</param>
  /// <returns>Normalized instance state.</returns>
  Task<VastInstance> GetInstanceAsync(string instanceId, CancellationToken cancellationToken = default);

  /// <summary>Creates a Vast instance from a selected offer id.</summary>
  /// <param name="offerId">Offer id returned by marketplace search.</param>
  /// <param name="request">Create-instance settings, including disk, image/template, env, and onstart fields.</param>
  /// <param name="cancellationToken">Cancels the HTTP request.</param>
  /// <returns>Created contract id returned by Vast.</returns>
  Task<VastCreateInstanceResult> CreateInstanceAsync(
    string                    offerId,
    VastCreateInstanceRequest request,
    CancellationToken         cancellationToken = default);

  /// <summary>Destroys a Vast instance.</summary>
  /// <param name="instanceId">Vast instance or contract id to destroy.</param>
  /// <param name="cancellationToken">Cancels the HTTP request.</param>
  Task DestroyInstanceAsync(string instanceId, CancellationToken cancellationToken = default);
}
