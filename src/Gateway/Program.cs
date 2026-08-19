using Serilog;
using Serilog.Events;
using Serilog.Formatting;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .WriteTo.File("logs/log-.log", rollingInterval: RollingInterval.Day)
        .ReadFrom.Configuration(context.Configuration);

    if (context.HostingEnvironment.IsDevelopment())
    {
        configuration.WriteTo.Console();
    }
    else
    {
        configuration.WriteTo.Console(new LokiJsonFormatter());
    }
});

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();


app.UseSerilogRequestLogging();
app.MapReverseProxy();

app.Logger.LogInformation("Gateway started");

app.Run();

// JSON у стилі caddy ({"level":"info","ts":...,"msg":"..."}), щоб Loki
// коректно визначав рівень логу замість "unk".
sealed class LokiJsonFormatter : ITextFormatter
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
