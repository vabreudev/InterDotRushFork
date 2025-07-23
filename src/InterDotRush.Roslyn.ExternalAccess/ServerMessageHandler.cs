using InterDotRush.Roslyn.ExternalAccess.Handlers;
using InterDotRush.Roslyn.ExternalAccess.Models;
using InterDotRush.Roslyn.Workspaces;

namespace InterDotRush.Roslyn.ExternalAccess;

public class ServerMessageHandler : IExternalTypeResolver
{
    private InterDotRushWorkspace workspace;

    public ServerMessageHandler(InterDotRushWorkspace workspace)
    {
        this.workspace = workspace;
    }

    public string? HandleResolveType(string identifierName, SourceLocation location)
    {
        return ExternalTypeResolver.Handle(identifierName, location, workspace.Solution);
    }
}
