using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace qDesk.Architecture.Tests;

/// <summary>
/// Executable versions of this solution's layering rules: Domain depends on nothing, Application
/// depends only on Domain, and no inner layer knows about WPF or the ORM. Without these tests,
/// "Application must not depend on EF Core" is a promise; with them it is a build failure.
/// </summary>
/// <remarks>
/// <para>
/// These tests read the compiled assemblies as *files*, using PEReader to parse their metadata
/// tables, rather than loading them into the test process with Assembly.Load. Two reasons: loading
/// executes module initializers and pins files for the lifetime of the process, and on a machine
/// with Windows Application Control enabled, loading a freshly built unsigned DLL is blocked
/// outright. Reading metadata needs no execution and no extra NuGet package, because
/// System.Reflection.Metadata ships in the shared framework.
/// </para>
/// <para>
/// One caveat worth knowing: the C# compiler omits references the code never uses, so an unused
/// reference will not appear here. These checks are therefore conservative, they can miss a
/// *declared* violation but never invent one. <see cref="ProjectReferenceTests"/> closes that gap by
/// reading the .csproj files instead.
/// </para>
/// </remarks>
public class LayerDependencyTests
{
    private const string DomainAssembly = "qDesk.Domain.dll";
    private const string ApplicationAssembly = "qDesk.Application.dll";
    private const string InfrastructureAssembly = "qDesk.Infrastructure.dll";

    private static readonly string[] WpfAssemblyNames =
    [
        "PresentationFramework",
        "PresentationCore",
        "WindowsBase",
        "System.Xaml",
    ];

    [Fact]
    public void Domain_references_no_other_qDesk_assembly()
    {
        var references = QDeskReferencesOf(DomainAssembly);

        Assert.Empty(references);
    }

    [Fact]
    public void Application_does_not_reference_Infrastructure()
    {
        var references = QDeskReferencesOf(ApplicationAssembly);

        Assert.DoesNotContain("qDesk.Infrastructure", references);
    }

    [Fact]
    public void Application_does_not_reference_EntityFrameworkCore()
    {
        // Persistence is Infrastructure's job. If EF Core types reach the Application layer, slices
        // start depending on the ORM and "persistence can be replaced" stops being true.
        var references = ReferenceNamesOf(ApplicationAssembly);

        Assert.DoesNotContain(
            references,
            name => name.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal));
    }

    [Fact]
    public void Inner_layers_do_not_reference_WPF()
    {
        string[] innerLayers = [DomainAssembly, ApplicationAssembly, InfrastructureAssembly];

        foreach (var assemblyFile in innerLayers)
        {
            var wpfReferences = ReferenceNamesOf(assemblyFile)
                .Intersect(WpfAssemblyNames, StringComparer.Ordinal)
                .ToArray();

            Assert.True(
                wpfReferences.Length == 0,
                $"{assemblyFile} references WPF: {string.Join(", ", wpfReferences)}");
        }
    }

    /// <summary>
    /// Reads the AssemblyRef metadata table of a built assembly without loading it.
    /// </summary>
    private static string[] ReferenceNamesOf(string assemblyFileName)
    {
        // Project references are copied next to the test binaries, so the built output of every
        // layer under test is already here.
        var assemblyPath = Path.Combine(AppContext.BaseDirectory, assemblyFileName);

        Assert.True(File.Exists(assemblyPath), $"Expected to find {assemblyFileName} next to the test binaries.");

        using var stream = File.OpenRead(assemblyPath);
        using var peReader = new PEReader(stream);
        var metadata = peReader.GetMetadataReader();

        return [.. metadata.AssemblyReferences
            .Select(handle => metadata.GetString(metadata.GetAssemblyReference(handle).Name))];
    }

    private static string[] QDeskReferencesOf(string assemblyFileName) =>
        [.. ReferenceNamesOf(assemblyFileName).Where(name => name.StartsWith("qDesk.", StringComparison.Ordinal))];
}
