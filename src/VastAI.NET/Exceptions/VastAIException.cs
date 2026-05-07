namespace VastAI.NET.Exceptions;

/// <summary>
///   Base exception for all VastAI library errors.
/// </summary>
/// <remarks>
///   <para>
///     This base exception allows callers to catch all library-specific errors
///     while still enabling specific exception handling for derived types.
///   </para>
/// </remarks>
public class VastAIException : Exception
{
  /// <summary>
  ///   Initializes a new instance of the <see cref="VastAIException" /> class.
  /// </summary>
  public VastAIException()
  {
  }


  /// <summary>
  ///   Initializes a new instance of the <see cref="VastAIException" /> class with a specified error message.
  /// </summary>
  /// <param name="message">The message that describes the error.</param>
  public VastAIException(string message)
    : base(message)
  {
  }


  /// <summary>
  ///   Initializes a new instance of the <see cref="VastAIException" /> class with a specified error message
  ///   and a reference to the inner exception that caused this exception.
  /// </summary>
  /// <param name="message">The message that describes the error.</param>
  /// <param name="innerException">
  ///   The exception that is the cause of the current exception, or a null reference if no inner exception is specified.
  /// </param>
  public VastAIException(string message, Exception innerException)
    : base(message, innerException)
  {
  }
}
