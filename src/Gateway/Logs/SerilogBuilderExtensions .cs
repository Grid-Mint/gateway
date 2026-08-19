using System;
using Serilog;

namespace Gateway.Logs;

public static class SerilogBuilderExtensions 
{
    public static WebApplicationBuilder AddSerilog(this WebApplicationBuilder builder)
    {
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

        return builder;
    }
}
