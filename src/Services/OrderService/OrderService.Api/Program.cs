using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderService.Api.Extensions;
using OrderService.Infrastructure.Context;
using Serilog;
using System;

namespace OrderService.Api
{
    public class Program
    {
        private static string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        private static IConfiguration configuration
        {
            get
            {
                return new ConfigurationBuilder()
                    .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                    .AddJsonFile($"Configurations/appsettings.json", optional: false)
                    .AddJsonFile($"Configurations/appsettings.{env}.json", optional: true)
                    .AddEnvironmentVariables()
                    .Build();
            }
        }

        private static IConfiguration serilogConfiguration
        {
            get
            {
                return new ConfigurationBuilder()
                    .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                    .AddJsonFile($"Configurations/serilog.json", optional: false)
                    .AddJsonFile($"Configurations/serilog.{env}.json", optional: true)
                    .AddEnvironmentVariables()
                    .Build();
            }
        }

        public static void Main(string[] args)
        {
            IHost host = CreateHostBuilder(args).Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(serilogConfiguration)
                .CreateLogger();

            host.MigrateDbContext<OrderDbContext>((context, services) =>
            {
                var logger = services.GetService<ILogger<OrderDbContext>>();

                var dbContextSeeder = new OrderDbContextSeed();
                dbContextSeeder.SeedAsync(context, logger)
                    .Wait();
            });

            Log.Information("Application is Running....");

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseDefaultServiceProvider((context, options) =>
                {
                    options.ValidateOnBuild = false;
                    options.ValidateScopes = false;
                })
                .ConfigureAppConfiguration(i => i.AddConfiguration(configuration))
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureLogging(i => i.ClearProviders())
                .UseSerilog();
    }
}
