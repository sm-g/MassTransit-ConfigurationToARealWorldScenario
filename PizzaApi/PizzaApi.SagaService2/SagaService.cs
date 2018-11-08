using System;
using Automatonymous;
using GreenPipes;
using MassTransit;
using MassTransit.BusConfigurators;
using MassTransit.EntityFrameworkCoreIntegration;
using MassTransit.EntityFrameworkCoreIntegration.Saga;
using Microsoft.EntityFrameworkCore;
using PizzaApi.MessageContracts;
using PizzaApi.StateMachines;
using Topshelf;

namespace PizzaApi.SagaService2
{
    public class SagaService : ServiceControl
    {
        private IBusControl _busControl;
        private IBusObserver _busObserver;

        //private BackgroundJobServer hangfireServer;
        private readonly OrderStateMachine _saga;

        private readonly IServiceProvider _serviceProvider;

        public SagaService(OrderStateMachine orderStateMachine, IServiceProvider serviceProvider)
        {
            _saga = orderStateMachine;
            _serviceProvider = serviceProvider;
        }

        private DbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<SagaDbContext<Order, OrderMap>>()
                .UseMySql("server=localhost;port=3306;database=pizza;uid=root;password=1111")
                .Options;
            return new SagaDbContext<Order, OrderMap>(options);
        }

        public bool Start(HostControl hostControl)
        {
            var repo = new EntityFrameworkSagaRepository<Order>(GetDbContext, optimistic: true);

            _busObserver = new BusObserver();

            _busControl = BusConfigurator.ConfigureBus((cfg, host) =>
            {
                cfg.AddBusFactorySpecification(new BusObserverSpecification(() => _busObserver));

                cfg.UseSerilog();
                // cfg.EnableWindowsPerformanceCounters();

                cfg.ConfigurePublish(x => x.UseSendFilter(new QbHeadersFilter<SendContext<IRequestCommand>, IRequestCommand>()));

                cfg.ReceiveEndpoint(host, RabbitMqConstants.SagaQueue, e =>
                {
                    e.UseConcurrencyLimit(16);
                    e.PrefetchCount = 16;

                    // required with optimistic concurrency
                    e.UseRetry(x => x.Intervals(TimeSpan.FromSeconds(5)));

                    e.UseCircuitBreaker(cb =>
                    {
                        cb.TripThreshold = 15;
                        cb.ResetInterval = TimeSpan.FromMinutes(5);
                        cb.TrackingPeriod = TimeSpan.FromMinutes(1);
                        cb.ActiveThreshold = 10;
                    });

                    e.StateMachineSaga(_saga, repo);
                    e.LoadFrom(_serviceProvider);
                });
            });

            var consumeObserver = new LogConsumeObserver();

            _busControl.ConnectConsumeObserver(consumeObserver);

            try
            {
                _busControl.Start();
                Console.WriteLine("Saga active.. Press enter to exit");

                //GlobalConfiguration.Configuration.UseMemoryStorage();
                //hangfireServer = new BackgroundJobServer();
                //Console.WriteLine("Hangfire Server started. Press any key to exit...");

                //WebApp.Start<Startup>("http://localhost:1235");
            }
            catch
            {
                _busControl.Stop();
                //hangfireServer?.Dispose();

                throw;
            }

            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            // graceful shutdown

            _busControl?.Stop(TimeSpan.FromMinutes(3));
            //hangfireServer?.Dispose();

            return true;
        }
    }
}