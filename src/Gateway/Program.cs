using Serilog;
using Serilog.Formatting.Json;

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
        configuration.WriteTo.Console(new JsonFormatter(renderMessage: true));
    }
});

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();


app.UseSerilogRequestLogging();
app.MapReverseProxy();

app.Logger.LogInformation("Gateway started");

app.Run();
