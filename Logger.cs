using StardewModdingAPI;

namespace BetterCraneGame;

internal static class Logger {

    static IMonitor Monitor = null!;
    static LogLevel logLevel => ModEntry.Config.Logging ? LogLevel.Info : LogLevel.Trace;

    internal static void Init(IMonitor monitor) {
        Monitor = monitor;
    }

    internal static void Log(string message) {
        Monitor.Log(message, logLevel);
    }

}