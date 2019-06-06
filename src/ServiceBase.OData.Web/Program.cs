using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace ServiceBase.OData.Web
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            const string AppName = "OData Example Application";

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Literate)
                .CreateLogger();
            
            try
            {
                // Only required if using NodaTime and Postgres
                NpgsqlConnection.GlobalTypeMapper.UseNodaTime();

                Log.Information($"Starting application {AppName}");
                CreateWebHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, $"Terminated unexpectedly : {AppName}");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }

            Log.Information($"Stopping application {AppName}");
            return 0;
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) => WebHost
            .CreateDefaultBuilder(args)
            .ConfigureLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog();
            })
            .UseStartup<Startup>();
    }
}
