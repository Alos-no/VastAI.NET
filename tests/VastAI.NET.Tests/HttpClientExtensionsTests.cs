namespace VastAI.NET.Tests;

using VastAI.NET.Http;

/// <summary>Tests the retry safety rules used by the DI HTTP resilience pipeline.</summary>
public sealed class HttpClientExtensionsTests
{
  /// <summary>PUT is not retried because Vast uses PUT to create paid instances.</summary>
  [Fact]
  public void IsRetryableMethod_ForPut_ReturnsFalse()
  {
    Assert.False(HttpClientExtensions.IsRetryableMethod(HttpMethod.Put));
  }

  /// <summary>DELETE is retried because cleanup must survive transient throttling.</summary>
  [Fact]
  public void IsRetryableMethod_ForDelete_ReturnsTrue()
  {
    Assert.True(HttpClientExtensions.IsRetryableMethod(HttpMethod.Delete));
  }
}
