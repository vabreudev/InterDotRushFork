using System.Diagnostics;

namespace InterDotRush.Common.Interop;

public static class ProcessExtensions
{
    // private const int ExitTimeout = 1000;

    public static void Terminate(this Process process)
    {
        if (!process.HasExited)
        {
            process.Kill();
            // process.WaitForExit(ExitTimeout);
        }
        process.Close();
    }
}