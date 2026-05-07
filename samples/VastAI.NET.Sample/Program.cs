using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VastAI.NET;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddVastAI(builder.Configuration);

using var host   = builder.Build();
var       client = host.Services.GetRequiredService<IVastApiClient>();

var instances = await client.GetInstancesAsync();
Console.WriteLine($"Visible Vast instances: {instances.Count}");
