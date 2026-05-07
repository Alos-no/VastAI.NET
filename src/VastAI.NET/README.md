# VastAI.NET

Typed .NET client for the Vast.ai REST API.

The library owns Vast request/response models, JSON parsing, authentication headers, and HTTP resilience registration. Application hosts should depend on `IVastApiClient` instead of invoking the `vastai` CLI or duplicating Vast DTOs.

For runtime-created clients, use `VastApiClient.Create(apiKey)` or pass a host-owned `HttpClient` with `VastApiClient.Create(apiKey, httpClient: httpClient)`.

Live API tests use test-project user secrets under `VastAI:ApiKey` and are marked with xUnit v3 `Explicit = true`.
