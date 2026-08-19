using Serilog.Events;
using Serilog.Formatting;
using System.Text.Json;

namespace Gateway.Logs;

public sealed class LokiJsonFormatter : ITextFormatter
{
    public void Format(LogEvent logEvent, TextWriter output)
    {
        var level = logEvent.Level switch
        {
            LogEventLevel.Verbose => "debug",
            LogEventLevel.Debug => "debug",
            LogEventLevel.Information => "info",
            LogEventLevel.Warning => "warn",
            LogEventLevel.Error => "error",
            LogEventLevel.Fatal => "fatal",
            _ => "info",
        };

        var payload = new Dictionary<string, object?>
        {
            ["level"] = level,
            ["ts"] = logEvent.Timestamp.ToUnixTimeMilliseconds() / 1000.0,
            ["msg"] = logEvent.RenderMessage(),
        };

        if (logEvent.Exception is not null)
        {
            payload["error"] = logEvent.Exception.ToString();
        }

        output.WriteLine(JsonSerializer.Serialize(payload));
    }
}
