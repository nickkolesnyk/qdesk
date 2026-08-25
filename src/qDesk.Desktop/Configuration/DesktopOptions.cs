namespace qDesk.Desktop.Configuration;

/// <summary>
/// Strongly typed view of the <c>Desktop</c> section of configuration.
/// </summary>
/// <remarks>
/// Reading configuration through a class like this rather than calling
/// <c>configuration["Desktop:ApplicationTitle"]</c> at the point of use buys three things: the key
/// is spelled once instead of at every call site, consumers depend on a type they can construct in a
/// test instead of on the configuration system, and a missing or misspelled section fails in one
/// place rather than silently yielding null somewhere far away.
/// </remarks>
public sealed class DesktopOptions
{
    /// <summary>
    /// Name of the configuration section this class binds to.
    /// </summary>
    public const string SectionName = "Desktop";

    /// <summary>
    /// Window and taskbar title. The default keeps the app launchable if configuration is missing.
    /// </summary>
    public string ApplicationTitle { get; init; } = "qDesk";
}
