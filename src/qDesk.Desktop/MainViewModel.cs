using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using qDesk.Desktop.Configuration;

namespace qDesk.Desktop;

/// <summary>
/// View model for the application shell.
/// </summary>
/// <remarks>
/// <para>
/// Nothing here references WPF. That is the property that makes view models testable: this class can
/// be constructed in a unit test with a stub <see cref="IOptions{TOptions}"/> and asserted on,
/// without starting a UI thread or creating a <c>Window</c>.
/// </para>
/// <para>
/// The values are plain get-only properties rather than change-notifying ones because nothing
/// changes them after construction. <c>INotifyPropertyChanged</c> earns its keep the moment a value
/// updates while the window is open, which is when CommunityToolkit.Mvvm will be introduced.
/// </para>
/// </remarks>
public sealed class MainViewModel(IOptions<DesktopOptions> options, IHostEnvironment environment)
{
    public string ApplicationTitle { get; } = options.Value.ApplicationTitle;

    /// <summary>
    /// Which configuration layers are active: Development, Production, and so on. Shown in the UI so
    /// that running against the wrong settings is visible rather than surprising.
    /// </summary>
    public string EnvironmentName { get; } = environment.EnvironmentName;
}
