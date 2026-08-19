using Gateway.Logs;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddSerilog();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();


app.UseSerilogRequestLogging();
app.MapReverseProxy();

app.Logger.LogInformation("Gateway started");

app.Run();
