using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace CaseManagement.ArchitectureTests;

/// <summary>
/// Locks in Clean Architecture project edges by reading each layer's .csproj on disk.
/// </summary>
public sealed class ProjectReferenceRulesTests
{
    private static string ApiRoot([CallerFilePath] string? thisFile = null)
    {
        var testsDir = Path.GetDirectoryName(thisFile!)!;
        return Path.GetFullPath(Path.Combine(testsDir, ".."));
    }

    [Theory]
    [MemberData(nameof(ExpectedProjectReferences))]
    public void Project_has_expected_project_references_only(string projectFileName, string[] expectedReferenceProjectNames)
    {
        var root = ApiRoot();
        var csprojPath = Path.Combine(root, Path.ChangeExtension(projectFileName, null), projectFileName);
        Assert.True(File.Exists(csprojPath), $"Missing project file: {csprojPath}");

        var doc = XDocument.Load(csprojPath);
        var actual = doc
            .Descendants("ProjectReference")
            .Select(e => e.Attribute("Include")?.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => ProjectReferenceName(v!))
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToArray();

        var expected = expectedReferenceProjectNames.OrderBy(n => n, StringComparer.Ordinal).ToArray();
        Assert.Equal(expected, actual);
    }

    public static TheoryData<string, string[]> ExpectedProjectReferences =>
        new()
        {
            { "CaseManagement.Domain.csproj", [] },
            { "CaseManagement.Application.csproj", ["CaseManagement.Domain"] },
            {
                "CaseManagement.Infrastructure.csproj",
                ["CaseManagement.Application", "CaseManagement.Domain"]
            },
            {
                "CaseManagement.Api.csproj",
                ["CaseManagement.Application", "CaseManagement.Infrastructure"]
            },
        };

    /// <summary>
    /// .csproj files use backslashes in Include paths; on Linux, <see cref="Path"/> APIs only
    /// treat '/' as a separator, so we normalize before taking the file name.
    /// </summary>
    private static string ProjectReferenceName(string include)
    {
        var normalized = include.Replace('\\', Path.AltDirectorySeparatorChar);
        return Path.GetFileNameWithoutExtension(normalized);
    }
}
