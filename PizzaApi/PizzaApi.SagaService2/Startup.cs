using Automatonymous;
using MassTransit.EntityFrameworkCoreIntegration;
using MassTransit.EntityFrameworkCoreIntegration.Saga;
using MassTransit.Saga;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PizzaApi.StateMachines;

namespace PizzaApi.SagaService2
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<SagaService>();
            services.AddSingleton<SagaStateMachine<Order>, OrderStateMachine>();

            services.AddScoped<ISagaRepository<Order>>(sp =>
            {
                return new EntityFrameworkSagaRepository<Order>(GetDbContext, optimistic: true);
            });
        }

        private DbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<SagaDbContext<Order, OrderMap>>()
                .UseMySql("server=localhost;port=3306;database=pizza;uid=root;password=1111")
                .Options;
            return new SagaDbContext<Order, OrderMap>(options);
        }
    }
}