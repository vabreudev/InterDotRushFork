using InterDotRush.Common.Interop;
using Mono.Debugging.Soft;

namespace InterDotRush.Debugging.Mono;

public class NoDebugLaunchAgent : BaseLaunchAgent
{
    public NoDebugLaunchAgent(LaunchConfiguration configuration) : base(configuration) { }
    public override void Prepare(DebugSession debugSession)
    {
        throw new NotSupportedException();
    }
    public override void Connect(SoftDebuggerSession session) { }
    public override IEnumerable<string> GetUserAssemblies(IProcessLogger? logger)
    {
        return Enumerable.Empty<string>();
    }
}