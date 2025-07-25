using NLog;

namespace InterDotRush.Common.Logging;

public static class CurrentSessionLogger
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    static CurrentSessionLogger()
    {
        LogConfig.InitializeLog();
    }

    public static void Error(Exception e)
    {
        logger.Error(e.ToString());
    }
    public static void Error(string message)
    {
        logger.Error(message);
    }
    public static void Debug(string message)
    {
        logger.Debug(message);
    }
}
