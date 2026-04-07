using System.Runtime.CompilerServices;
using FluentAssertions;

public static class FluentAssertionsSetup
{
    [ModuleInitializer]
    public static void Initialize()
    {
        License.Accepted = true;
    }
}