namespace VastAI.NET.Models;

/// <summary>Result returned after Vast accepts a create-instance request.</summary>
/// <param name="InstanceId">New contract or instance id returned by Vast.</param>
public sealed record VastCreateInstanceResult(string InstanceId);
