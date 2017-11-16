using Serilog;
using System.Linq;
using System;
using Topshelf;

namespace PizzaDesktopApp.Attendant
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                //.WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss}|{ThreadId:00}|{Level:u3}{EventId} {SourceContext}{Scope}|{Message}{NewLine}{Exception}")
                .WriteTo.Trace()
                .WriteTo.File(".\\..\\..\\..\\logs\\attendant.log",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:HH:mm:ss}|{ThreadId:00}|{Level:u3}{EventId} {SourceContext}{Scope}|{Message}{NewLine}{Exception}")
                .Enrich.FromLogContext()
                .MinimumLevel.Information()
                .CreateLogger();

            return (int)HostFactory.Run(x => x.Service<AttendantService>());
        }
    }
}