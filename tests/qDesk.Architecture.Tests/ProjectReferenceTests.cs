using System.Xml.Linq;

namespace qDesk.Architecture.Tests;

/// <summary>
/// Reads the .csproj files directly and asserts the dependency graph that is *declared*, rather than
/// the one that survives into compiled output. This catches a wrong reference the moment it is added,
/// before any code uses it, which is exactly when it is cheap to remove.
/// </summary>
public class ProjectReferenceTests
{
    private static readonly string RepositoryRoot = FindRepositoryRoot();

    [Fact]
    public void Domain_declares_no_project_references()
    {
        Assert.Empty(ProjectReferencesOf("qDesk.Domain"));
    }

    [Fact]
    public void Application_references_only_Domain()
    {
        string[] expected = ["qDesk.Domain"];

        Assert.Equal(expected, ProjectReferencesOf("qDesk.Application"));
    }

    [Fact]
    public void Infrastructure_references_only_Application()
    {
        // Infrastructure reaches Domain transitively, through Application. Adding a direct
        // reference here would be harmless today and confusing later, so it is disallowed.
        string[] expected = ["qDesk.Application"];

        Assert.Equal(expected, ProjectReferencesOf("qDesk.Infrastructure"));
    }

    [Fact]
    public void Only_the_desktop_project_is_windows_specific()
    {
        var windowsSpecificProjects = SourceProjectFiles()
            .Where(path => TargetFrameworkOf(path).Contains("-windows", StringComparison.Ordinal))
            .Select(Path.GetFileNameWithoutExtension)
            .Order(StringComparer.Ordinal)
            .ToArray();

        string[] expected = ["qDesk.Desktop"];

        Assert.Equal(expected, windowsSpecificProjects);
    }

    private static string[] ProjectReferencesOf(string projectName)
    {
        var projectFile = Path.Combine(RepositoryRoot, "src", projectName, $"{projectName}.csproj");
        var document = XDocument.Load(projectFile);

        return document.Descendants("ProjectReference")
            .Select(element => ProjectNameFrom(element.Attribute("Include")!.Value))
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    /// <summary>
    /// Turns an MSBuild Include path such as <c>..\qDesk.Domain\qDesk.Domain.csproj</c> into
    /// <c>qDesk.Domain</c>.
    /// </summary>
    /// <remarks>
    /// MSBuild writes these paths with backslashes on every platform, but <see cref="Path"/> treats a
    /// backslash as a separator only on Windows. Passing the raw value to
    /// <see cref="Path.GetFileNameWithoutExtension(string)"/> therefore returns the project name on
    /// Windows and the entire relative path on Linux. Normalising to forward slashes first is
    /// unambiguous on both, because Windows accepts them as separators too.
    /// </remarks>
    private static string ProjectNameFrom(string includePath) =>
        Path.GetFileNameWithoutExtension(includePath.Replace('\\', '/'));

    private static string TargetFrameworkOf(string projectFile) =>
        XDocument.Load(projectFile).Descendants("TargetFramework").Single().Value;

    private static IEnumerable<string> SourceProjectFiles() =>
        Directory.EnumerateFiles(Path.Combine(RepositoryRoot, "src"), "*.csproj", SearchOption.AllDirectories)
            // WPF's XAML compilation step writes throwaway "_wpftmp" project files next to the real
            // one, and build output directories can hold stale copies. Neither is a real project.
            .Where(path => !path.Contains("_wpftmp", StringComparison.Ordinal))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal));

    /// <summary>
    /// Walks up from the test binaries until it finds the solution file. Tests must not assume the
    /// working directory, because it differs between `dotnet test`, an IDE runner, and CI.
    /// </summary>
    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "qDesk.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException(
                "Could not locate the repository root: no qDesk.slnx found above the test output directory.");
    }
}
