using System;
using GreenPipes;
using MassTransit;
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

        private readonly IServiceProvider _serviceProvider;

        public SagaService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public bool Start(HostControl hostControl)
        {
            _busObserver = new BusObserver();

            _busControl = BusConfigurator.ConfigureBus((cfg, host) =>
            {
                cfg.BusObserver(_busObserver);

                cfg.UseSerilog();
                // cfg.EnableWindowsPerformanceCounters();

                cfg.ConfigurePublish(x => x.UseSendFilter(new QbHeadersFilter<SendContext<IRequestCommand>, IRequestCommand>()));

                cfg.ReceiveEndpoint(host, RabbitMqConstants.SagaQueue, e =>
                {
                    e.UseConcurrencyLimit(2);
                    e.PrefetchCount = 2;

                    // required with optimistic concurrency
                    e.UseRetry(x => x.Intervals(TimeSpan.FromSeconds(5)));

                    e.UseCircuitBreaker(cb =>
                    {
                        cb.TripThreshold = 15;
                        cb.ResetInterval = TimeSpan.FromMinutes(5);
                        cb.TrackingPeriod = TimeSpan.FromMinutes(1);
                        cb.ActiveThreshold = 10;
                    });

                    e.StateMachineSaga<Order>(_serviceProvider);
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