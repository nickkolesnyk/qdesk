using System.Windows;

namespace qDesk.Desktop;

/// <summary>
/// The application shell.
/// </summary>
/// <remarks>
/// The constructor takes its view model instead of creating one. That is what removing
/// <c>StartupUri</c> from App.xaml bought: WPF is no longer the thing constructing this window, so
/// it no longer needs a parameterless constructor, and the dependency is visible in the signature
/// rather than hidden in the body.
/// </remarks>
public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
    }
}
