using Microsoft.Extensions.Logging;

namespace qDesk.Desktop.Logging;

/// <summary>
/// Compile-time generated log methods for application lifetime events.
/// </summary>
/// <remarks>
/// <para>
/// Calling <c>logger.LogInformation("... {Value}", value)</c> looks harmless but goes through a
/// <c>params object?[]</c> overload: the array is allocated and each value type argument is boxed on
/// every call, before the logger has even decided whether the level is enabled. Analyzer CA1848
/// flags exactly that, and the build treats it as an error.
/// </para>
/// <para>
/// The <see cref="LoggerMessageAttribute"/> source generator writes the implementation of each
/// <c>partial</c> method below at compile time: strongly typed parameters, no array, no boxing, and
/// an enabled-check before any formatting work. The message template still produces structured
/// fields, so <c>EnvironmentName</c> stays queryable rather than being flattened into a string.
/// </para>
/// </remarks>
internal static partial class AppLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "qDesk started in the {EnvironmentName} environment.")]
    public static partial void Started(ILogger logger, string environmentName);

    /// <remarks>
    /// The generator treats an <see cref="Exception"/> parameter specially: it is attached to the log
    /// entry as the exception rather than formatted into the message, so the stack trace survives.
    /// </remarks>
    [LoggerMessage(
        Level = LogLevel.Critical,
        Message = "qDesk failed to start.")]
    public static partial void StartupFailed(ILogger logger, Exception exception);
}
