namespace VastAI.NET.Models;

/// <summary>Settings sent to Vast when creating an instance from a marketplace offer.</summary>
public sealed class VastCreateInstanceRequest
{
  /// <summary>Rented disk size in GB.</summary>
  public int DiskGb { get; set; }

  /// <summary>Label assigned to the instance for later safety checks and diagnostics.</summary>
  public string Label { get; set; } = "";

  /// <summary>Container image to use when no template hash is supplied.</summary>
  public string Image { get; set; } = "";

  /// <summary>Optional Vast template hash id.</summary>
  public string TemplateHashId { get; set; } = "";

  /// <summary>Vast runtime type. This workflow uses SSH-capable containers.</summary>
  public string RuntimeType { get; set; } = "ssh";

  /// <summary>
  ///   Environment variables and port mappings sent to Vast's create-instance endpoint.
  ///   Vast REST expects a JSON object where variables are normal key/value pairs and port mappings use keys such as
  ///   <c>-p 8080:8080</c> with value <c>1</c>.
  /// </summary>
  public Dictionary<string, string> Environment { get; set; } = [];

  /// <summary>Optional onstart shell command executed by Vast inside the rented container.</summary>
  public string OnStart { get; set; } = "";

  /// <summary>Whether Vast should cancel the rental when the selected offer is no longer available.</summary>
  public bool CancelUnavailable { get; set; } = true;
}
