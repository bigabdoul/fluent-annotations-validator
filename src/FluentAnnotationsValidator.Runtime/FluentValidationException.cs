namespace FluentAnnotationsValidator.Runtime;

/// <summary>
/// Represents errors that occur during fluent validation configuration or execution.
/// </summary>
public class FluentValidationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FluentValidationException"/> class.
    /// </summary>
    public FluentValidationException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentValidationException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public FluentValidationException(string? message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentValidationException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public FluentValidationException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
