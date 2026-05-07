# VastAI.NET [![.NET Build and Test](https://github.com/Alos-no/VastAI.NET/actions/workflows/CI.yml/badge.svg)](https://github.com/Alos-no/VastAI.NET/actions/workflows/CI.yml) [![License: Apache-2.0](https://img.shields.io/badge/license-Apache--2.0-27ae60)](https://github.com/Alos-no/VastAI.NET/blob/main/LICENSE.txt) [![NuGet (VastAI.NET)](https://img.shields.io/nuget/v/VastAI.NET?label=VastAI.NET&color=27ae60)](https://www.nuget.org/packages/VastAI.NET/)

`VastAI.NET` is a typed .NET client for the Vast.ai REST API.

## Install

```bash
dotnet add package VastAI.NET
```

## Configure

```json
{
  "VastAI": {
    "ApiKey": "<vast-api-key>",
    "ApiBaseUri": "https://console.vast.ai/"
  }
}
```

The API key is required for live calls. Keep it in user secrets, environment variables, or host-specific secret storage.

## Use With DI

```csharp
using VastAI.NET;

builder.Services.AddVastAI(builder.Configuration);

var client = provider.GetRequiredService<IVastApiClient>();
var instances = await client.GetInstancesAsync();
```

## Use With Runtime Values

Use this path when the API key is entered after startup, such as in a browser app or interactive CLI:

```csharp
using VastAI.NET;

var client = VastApiClient.Create(apiKeyFromUserInput);
var instances = await client.GetInstancesAsync();
```

Hosts that already own an `HttpClient` can pass it:

```csharp
var client = VastApiClient.Create(apiKeyFromUserInput, httpClient: httpClient);
```

## API Usage

### SearchOffersAsync

Use this to find rentable offers before creating an instance. Filters use Vast field names and comparison operators.

```csharp
using VastAI.NET.Models;

var search = new VastSearchOffersRequest
{
  Limit = 5,
  Order = "dph_total"
};
search.Filters["rentable"] = new VastSearchFilter { EqualTo = true };
search.Filters["verified"] = new VastSearchFilter { EqualTo = true };
search.Filters["gpu_name"] = new VastSearchFilter { In = ["RTX_3060", "RTX_4090"] };
search.Filters["dph_total"] = new VastSearchFilter { LessThanOrEqualTo = 0.20 };
search.Filters["inet_up_cost"] = new VastSearchFilter { LessThanOrEqualTo = 1.0 / 1024.0 };
search.Filters["inet_down_cost"] = new VastSearchFilter { LessThanOrEqualTo = 1.0 / 1024.0 };

var offers = await client.SearchOffersAsync(search);
```

`Order` accepts compact Vast-style sort expressions: `dph_total` sorts ascending, `dlperf_usd-` sorts descending.

### GetInstancesAsync

Use this for safety checks before renting and for account-level monitoring.

```csharp
var instances = await client.GetInstancesAsync();
var active = instances.Where(instance => instance.IsActive).ToList();
```

`VastInstance` includes normalized status, label, GPU name, hourly price, estimated age, estimated cost, and SSH target fields when Vast returns them.

### GetInstanceAsync

Use this after create, while waiting for boot, or when refreshing one known instance.

```csharp
var instance = await client.GetInstanceAsync(instanceId);

if (instance.HasSshTarget)
{
  Console.WriteLine($"{instance.SshUser}@{instance.SshHost}:{instance.SshPort}");
}
```

The parser handles Vast responses where `instances` is either an array or a single object.

### CreateInstanceAsync

Use this only after selecting an offer and passing your own safety gates. This is a paid operation.

```csharp
var create = new VastCreateInstanceRequest
{
  DiskGb = 20,
  Label = "lfs-smoke-test",
  Image = "vastai/base-image:@vastai-automatic-tag",
  RuntimeType = "ssh",
  CancelUnavailable = true,
  OnStart = "echo started"
};
create.Environment["-p 8080:8080"] = "1";
create.Environment["DATA_DIRECTORY"] = "/workspace/";

var result = await client.CreateInstanceAsync(offer.Id, create);
Console.WriteLine(result.InstanceId);
```

Vast REST requires `Environment` as a JSON object. Port mappings use keys such as `-p 8080:8080` with value `1`; environment variables use normal key/value pairs.

### DestroyInstanceAsync

Use this to stop billing for an instance. Always call it from a `finally` block in tests or automation that rented an instance.

```csharp
try
{
  var result = await client.CreateInstanceAsync(offer.Id, create);
  instanceId = result.InstanceId;
}
finally
{
  if (!string.IsNullOrWhiteSpace(instanceId))
    await client.DestroyInstanceAsync(instanceId);
}
```

The client uses bearer-token auth and the official Vast REST endpoints under `https://console.vast.ai/`.

## Live Tests

Live tests are xUnit v3 explicit tests. They do not run during the normal suite.

```powershell
dotnet user-secrets set "VastAI:ApiKey" "<vast-api-key>" --project .\tests\VastAI.NET.Tests\VastAI.NET.Tests.csproj
dotnet test .\tests\VastAI.NET.Tests\VastAI.NET.Tests.csproj --filter LiveVastApiTests -- xUnit.Explicit=on
```

`CreateGetInstanceDestroyAsync_WithCheapLiveOffer_RentsAndDestroysInstance` is paid. It rents one offer capped at `$0.20/hr`, destroys it in a `finally` block, and fails if active instances already exist.
