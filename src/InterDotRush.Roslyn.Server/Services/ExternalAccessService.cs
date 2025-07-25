using InterDotRush.Roslyn.ExternalAccess;

namespace InterDotRush.Roslyn.Server.Services;

public class ExternalAccessService
{
    private readonly RpcServerHost rpcServerHost;

    public ExternalAccessService(WorkspaceService workspaceService)
    {
        rpcServerHost = new RpcServerHost(workspaceService);
    }

    public Task StartListeningAsync(int? transportId, CancellationToken cancellationToken)
    {
        if (transportId == null || transportId <= 0)
            return Task.CompletedTask;

        return rpcServerHost.RunAsync($"InterDotRush-{transportId}", cancellationToken);
    }
}