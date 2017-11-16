using Hangfire;

using Hangfire.MemoryStorage;

using Microsoft.Owin;
using Owin;
using System;
using System.Linq;

[assembly: OwinStartup(typeof(PizzaApi.WindowsService.Startup))]

namespace PizzaApi.WindowsService
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var storage = new MemoryStorage();

            app.UseHangfireDashboard("/hangfire-masstransit", new DashboardOptions(), storage);
        }
    }
}