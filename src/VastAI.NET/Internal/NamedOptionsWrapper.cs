namespace VastAI.NET.Internal;

using Microsoft.Extensions.Options;

/// <summary>
///   Wraps a resolved options instance for injection into components expecting <see cref="IOptions{TOptions}" />.
/// </summary>
/// <typeparam name="TOptions">The options type.</typeparam>
/// <remarks>
///   <para>
///     This adapter is useful when you have a resolved options instance (e.g., from a named options monitor)
///     and need to pass it to a component that expects <see cref="IOptions{TOptions}" />.
///   </para>
/// </remarks>
/// <example>
///   <code>
///   // In a factory creating clients for named configurations:
///   var namedOptions = optionsMonitor.Get(clientName);
///   var wrappedOptions = new NamedOptionsWrapper&lt;VastAIOptions&gt;(namedOptions);
///   var client = new VastAIClient(wrappedOptions, logger);
///   </code>
/// </example>
internal sealed class NamedOptionsWrapper<TOptions>(TOptions value) : IOptions<TOptions>
  where TOptions : class
{
  /// <summary>
  ///   Gets the options value.
  /// </summary>
  public TOptions Value { get; } = value ?? throw new ArgumentNullException(nameof(value));
}
