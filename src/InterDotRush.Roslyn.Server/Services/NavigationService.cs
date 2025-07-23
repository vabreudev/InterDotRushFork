using InterDotRush.Common.Extensions;
using InterDotRush.Roslyn.Navigation;
using Microsoft.CodeAnalysis;

namespace InterDotRush.Roslyn.Server.Services;

public class NavigationService
{
    private readonly NavigationHost navigationHost;
    public Solution? Solution => navigationHost.Solution;

    public NavigationService()
    {
        navigationHost = new NavigationHost();
    }

    public Task<string?> EmitDecompiledFileAsync(ISymbol symbol, Project project, CancellationToken cancellationToken)
    {
        return SafeExtensions.InvokeAsync(default(string), async () =>
        {
            return await navigationHost.EmitDecompiledFileAsync(symbol, project, cancellationToken).ConfigureAwait(false);
        });
    }
    public Task<string?> EmitCompilerGeneratedFileAsync(Location location, Project project, CancellationToken cancellationToken)
    {
        return SafeExtensions.InvokeAsync(default(string), async () =>
        {
            return await navigationHost.EmitCompilerGeneratedFileAsync(location, project, cancellationToken).ConfigureAwait(false);
        });
    }
    public void UpdateSolution(Solution? solution)
    {
        navigationHost.Solution = solution;
    }
}