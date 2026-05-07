namespace VastAI.NET.Internal;

/// <summary>
///   Centralized logging category constants for the VastAI library.
/// </summary>
/// <remarks>
///   <para>
///     Using centralized category names ensures:
///     <list type="bullet">
///       <item>DRY principle - single source of truth for category names</item>
///       <item>Consistent logging across all library components</item>
///       <item>Independent verbosity tuning per component via logging configuration</item>
///       <item>Structured log filtering and analysis</item>
///     </list>
///   </para>
/// </remarks>
/// <example>
///   <code>
///   // In appsettings.json, configure per-category log levels:
///   {
///     "Logging": {
///       "LogLevel": {
///         "VastAI.NET.Core": "Information",
///         "VastAI.NET.Http": "Warning",
///         "VastAI.NET.Http.Resilience": "Debug"
///       }
///     }
///   }
///   </code>
/// </example>
internal static class LoggingConstants
{
  /// <summary>
  ///   Logging category names for different library components.
  /// </summary>
  public static class Categories
  {
    /// <summary>Core service logging category.</summary>
    public const string Core = "VastAI.NET.Core";

    /// <summary>HTTP client logging category.</summary>
    public const string Http = "VastAI.NET.Http";

    /// <summary>HTTP resilience pipeline logging category (retries, circuit breaker, etc.).</summary>
    public const string HttpResilience = "VastAI.NET.Http.Resilience";

    /// <summary>Configuration and options logging category.</summary>
    public const string Configuration = "VastAI.NET.Configuration";

    /// <summary>Factory and client creation logging category.</summary>
    public const string Factory = "VastAI.NET.Factory";
  }


  /// <summary>
  ///   Event IDs for structured logging.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Event IDs enable structured log filtering and alerting.
  ///     Reserve ranges for different components:
  ///     <list type="bullet">
  ///       <item>100-199: Core service events</item>
  ///       <item>200-299: HTTP client events</item>
  ///       <item>300-399: Configuration events</item>
  ///       <item>400-499: Factory events</item>
  ///       <item>500-599: Error events</item>
  ///     </list>
  ///   </para>
  /// </remarks>
  public static class EventIds
  {
    // Core service events (100-199)
    public const int ProcessingStarted = 100;
    public const int ProcessingCompleted = 101;
    public const int ProcessingSkipped = 102;

    // HTTP client events (200-299)
    public const int HttpRequestStarted = 200;
    public const int HttpRequestCompleted = 201;
    public const int HttpRequestFailed = 202;
    public const int HttpRetryAttempt = 210;
    public const int HttpCircuitBreakerOpened = 220;
    public const int HttpCircuitBreakerClosed = 221;
    public const int HttpRateLimited = 230;

    // Configuration events (300-399)
    public const int ConfigurationLoaded = 300;
    public const int ConfigurationValidationFailed = 301;

    // Factory events (400-499)
    public const int ClientCreated = 400;
    public const int ClientDisposed = 401;

    // Error events (500-599)
    public const int UnexpectedError = 500;
    public const int OperationCancelled = 501;
  }
}
