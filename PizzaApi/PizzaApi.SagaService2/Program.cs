using Topshelf;
using System;
using System.Linq;
using Serilog;
using Serilog.Filters;
using Topshelf.Runtime;
using Microsoft.Extensions.DependencyInjection;
using PizzaApi.StateMachines;

namespace PizzaApi.WindowsService
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

            var serviceProvider = ConfigureServices();

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

        private static ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddScoped<SagaService>()
                .AddScoped<OrderStateMachine>()
                .BuildServiceProvider();
        }
    }
}