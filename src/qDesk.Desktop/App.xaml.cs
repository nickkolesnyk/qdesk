using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using qDesk.Desktop.Configuration;
using qDesk.Desktop.Logging;

namespace qDesk.Desktop;

/// <summary>
/// Application entry point and composition root. The other half of this class is generated from
/// App.xaml at build time.
/// </summary>
/// <remarks>
/// <para>
/// The base type is written out in full as <c>System.Windows.Application</c> rather than
/// <c>Application</c>. Inside namespace <c>qDesk.Desktop</c>, C# resolves the bare name
/// <c>Application</c> by walking outwards through enclosing namespaces, finds the <c>qDesk</c>
/// namespace, and there discovers our own <c>qDesk.Application</c> project namespace, which wins
/// over anything brought in by a <c>using</c>. A namespace cannot be a base class, hence the
/// compiler error. Qualifying the name is the least surprising fix.
/// </para>
/// <para>
/// This is the only place in the application allowed to know how objects are constructed. Every
/// other type declares what it needs as constructor parameters and receives it.
/// </para>
/// </remarks>
public partial class App : System.Windows.Application
{
    private readonly IHost _host;
    private readonly string _environmentName;

    public App()
    {
        // The content root is where relative configuration file paths are resolved from. It defaults
        // to the current working directory, which for a desktop app is whatever directory the user
        // happened to launch from — Explorer, a shortcut, or a shell. Pointing it at the directory
        // holding the executable is the only way appsettings.json is found reliably.
        HostApplicationBuilderSettings settings = new()
        {
            ContentRootPath = AppContext.BaseDirectory,
        };

        HostApplicationBuilder builder = Host.CreateApplicationBuilder(settings);

        _environmentName = builder.Environment.EnvironmentName;

        // Local overrides, git-ignored, layered last so they win. This is where a developer's own
        // database credentials will go, which is why the file must never be committed.
        builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false);

        builder.Services.Configure<DesktopOptions>(
            builder.Configuration.GetSection(DesktopOptions.SectionName));

        // Singleton: one shell window and one view model for the lifetime of the process. Feature
        // view models will be Transient instead, because each navigation wants a fresh one.
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<MainWindow>();

        _host = builder.Build();
    }

    /// <remarks>
    /// <para>
    /// <c>async void</c> is normally a bug, because the caller cannot await it and an escaping
    /// exception is raised on the synchronization context instead of being observed. It is
    /// deliberate here: WPF declares this override as returning <c>void</c>, so there is nothing to
    /// return a task to. That makes the try/catch mandatory rather than defensive — without it, a
    /// failure while starting the host would terminate the process with no explanation.
    /// </para>
    /// <para>
    /// Blocking instead of awaiting would be worse: the host's startup continuations post back to
    /// the UI thread, so waiting on them from that same thread can deadlock.
    /// </para>
    /// </remarks>
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            await _host.StartAsync();

            // Both arguments are resolved before the call rather than inline. Arguments to a logging
            // method are evaluated at the call site, before the generated method checks whether the
            // level is enabled, so a service lookup there is work done for a message that may be
            // discarded. Analyzer CA1873 enforces this.
            var logger = _host.Services.GetRequiredService<ILogger<App>>();

            AppLog.Started(logger, _environmentName);

            _host.Services.GetRequiredService<MainWindow>().Show();
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                $"qDesk failed to start.\n\n{exception}",
                "qDesk",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown(1);
        }
    }

    /// <remarks>
    /// Shutdown blocks on purpose, because the process must not die before hosted services have
    /// stopped. <see cref="Task.Run{TResult}(Func{Task{TResult}})"/> moves the wait off the UI
    /// thread first: awaiting inside <c>StopAsync</c> would otherwise try to resume on a UI thread
    /// that is blocked waiting for it, which is the classic sync-over-async deadlock.
    /// </remarks>
    protected override void OnExit(ExitEventArgs e)
    {
        Task.Run(() => _host.StopAsync()).GetAwaiter().GetResult();
        _host.Dispose();

        base.OnExit(e);
    }
}
