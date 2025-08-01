using System.IO.Pipes;
using InterDotRush.Common.Logging;
using InterDotRush.Roslyn.Workspaces;
using StreamJsonRpc;

namespace InterDotRush.Roslyn.ExternalAccess;

public class RpcServerHost
{
    private readonly CurrentClassLogger currentClassLogger;
    private readonly ServerMessageHandler messageHandler;

    public RpcServerHost(InterDotRushWorkspace workspace)
    {
        this.currentClassLogger = new CurrentClassLogger(nameof(RpcServerHost));
        this.messageHandler = new ServerMessageHandler(workspace);
    }

    public async Task RunAsync(string transportId, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            using var pipeStream = new NamedPipeServerStream(transportId, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            currentClassLogger.Debug($"Server created with transport id: '{transportId}'");
            currentClassLogger.Debug("Waiting for connection...");

            await pipeStream.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
            currentClassLogger.Debug("Client connected");

            using var jsonRpc = JsonRpc.Attach(pipeStream, messageHandler);
            await jsonRpc.Completion.ConfigureAwait(false);
        }
    }
}