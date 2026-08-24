using System.Reflection;
using System.Runtime.Versioning;

namespace qDesk.Domain.Tests;

/// <summary>
/// Proves the test harness itself works: the runner discovers tests and the project reference to
/// qDesk.Domain resolves. Real domain behaviour is tested in the files that follow this one.
/// </summary>
public class SolutionLayoutTests
{
    [Fact]
    public void Domain_assembly_is_reachable_from_its_test_project()
    {
        var domainAssembly = typeof(AssemblyMarker).Assembly;

        Assert.Equal("qDesk.Domain", domainAssembly.GetName().Name);
    }

    [Fact]
    public void Domain_assembly_targets_a_platform_agnostic_framework()
    {
        // If qDesk.Domain is ever retargeted to net10.0-windows, WPF becomes reachable from the
        // domain and the layering argument collapses. This is the cheap early warning; the
        // architecture tests will cover the dependency rules properly.
        var targetFramework = typeof(AssemblyMarker).Assembly
            .GetCustomAttribute<TargetFrameworkAttribute>();

        Assert.NotNull(targetFramework);
        Assert.DoesNotContain("windows", targetFramework.FrameworkName, StringComparison.OrdinalIgnoreCase);
    }
}
