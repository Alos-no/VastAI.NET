# VastAI.NET

Typed .NET client for the Vast.ai REST API.

The library provides typed request/response models, authentication headers, JSON mapping, runtime client creation, and optional HTTP resilience registration.

For runtime-created clients, use `VastApiClient.Create(apiKey)` or pass a host-owned `HttpClient` with `VastApiClient.Create(apiKey, httpClient: httpClient)`.

Live API tests use test-project user secrets under `VastAI:ApiKey` and are marked with xUnit v3 `Explicit = true`.
