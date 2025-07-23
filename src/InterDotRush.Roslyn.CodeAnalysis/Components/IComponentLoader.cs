using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace InterDotRush.Roslyn.CodeAnalysis.Components;

public interface IComponentLoader<T> where T : class
{
    MemoryCache<T> ComponentsCache { get; }

    List<T> LoadFromProject(Project project);
    List<T> LoadFromAssembly(string assemblyName);
    List<T> LoadFromInterDotRush();
    ImmutableArray<T> GetComponents(Project project);
}