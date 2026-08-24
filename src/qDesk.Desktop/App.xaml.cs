namespace qDesk.Desktop;

/// <summary>
/// Application entry point. The other half of this class is generated from App.xaml at build time.
/// </summary>
/// <remarks>
/// The base type is written out in full as <c>System.Windows.Application</c> rather than
/// <c>Application</c>. Inside namespace <c>qDesk.Desktop</c>, C# resolves the bare name
/// <c>Application</c> by walking outwards through enclosing namespaces, finds the <c>qDesk</c>
/// namespace, and there discovers our own <c>qDesk.Application</c> project namespace, which wins
/// over anything brought in by a <c>using</c>. A namespace cannot be a base class, hence the
/// compiler error. Qualifying the name is the least surprising fix.
/// </remarks>
public partial class App : System.Windows.Application
{
}
