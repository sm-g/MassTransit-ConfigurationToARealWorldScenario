using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Topshelf;

namespace PizzaApi.SagaService2
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                //.WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss}|{ThreadId:00}|{Level:u3}{EventId} {SourceContext}{Scope}|{Message}{NewLine}{Exception}")
                .WriteTo.Trace()
                .WriteTo.File(".\\..\\..\\..\\logs\\win-service.log",
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: "{Timestamp:HH:mm:ss}|{ThreadId:00}|{Level:u3}{EventId} {SourceContext}{Scope}|{Message}{NewLine}{Exception}")
                .Enrich.FromLogContext()
                .MinimumLevel.Information()
                .CreateLogger();

            var contentRoot = Directory.GetCurrentDirectory();

            var configuration = BuildConfiguration(contentRoot);

            var serviceProvider = BuildServiceProvider(configuration);

            HostFactory.Run(x =>
            {
                x.Service(() => serviceProvider.GetRequiredService<SagaService>());

                x.UseSerilog();
                x.DependsOn("RabbitMQ");

                x.RunAsLocalSystem();
                x.StartAutomatically();

                x.SetDescription("Saga Service - Pizza API Order State Machine");
                x.SetDisplayName("Saga Service - Pizza API Order State Machine");
                x.SetServiceName("Saga Service - Pizza API Order State Machine");

                //x.EnablePauseAndContinue();

                x.EnableServiceRecovery(r =>
                {
                    r.RestartService(1);

                    //number of days until the error count resets
                    r.SetResetPeriod(1);
                });
            });

            //TODO:Enable Pause and continue (Stop the bus but don't stop the hangfire server)
            //Console.ReadLine();
        }

        public static IConfigurationRoot BuildConfiguration(string contentRoot)
        {
            return new ConfigurationBuilder()
                .SetBasePath(contentRoot)
                .AddJsonFile($"Configuration/appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }

        public static ServiceProvider BuildServiceProvider(IConfigurationRoot configuration)
        {
            var serviceCollection = new ServiceCollection();

            var startup = new Startup(configuration);
            startup.ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            return serviceProvider;
        }
    }
}