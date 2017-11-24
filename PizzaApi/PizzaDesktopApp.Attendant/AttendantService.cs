using System;
using GreenPipes;
using MassTransit;
using PizzaApi.MessageContracts;
using System.Linq;
using Topshelf;
using Topshelf.Logging;

namespace PizzaDesktopApp.Attendant
{
    public class AttendantService : ServiceControl
    {
        private ConnectHandle _connectHandle;
        private IBusControl _busControl;

        public bool Start(HostControl hostControl)
        {
            _busControl = BusConfigurator.ConfigureBus((cfg, host) =>
            {
                cfg.UseSerilog();
                cfg.UseRetry(x => x.Immediate(1));
                cfg.ReceiveEndpoint(host, RabbitMqConstants.RegisterOrderServiceQueue, e =>
                {
                    e.UseConcurrencyLimit(4);
                    e.PrefetchCount = 4;

                    e.UseRateLimit(1, TimeSpan.FromSeconds(30));

                    e.UseRetry(x => x.Interval(5, TimeSpan.FromSeconds(5)));

                    e.UseCircuitBreaker(cb =>
                    {
                        cb.TripThreshold = 15;
                        cb.ResetInterval = TimeSpan.FromMinutes(5);
                        cb.TrackingPeriod = TimeSpan.FromMinutes(1);
                        cb.ActiveThreshold = 10;
                    });

                    e.Consumer<OrderRegisteredConsumer>();
                });
            });

            var consumeObserver = new LogConsumeObserver();
            _connectHandle = _busControl.ConnectConsumeObserver(consumeObserver);

            _busControl.Start();

            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            _connectHandle?.Disconnect();
            _busControl?.Stop();

            return true;
        }
    }
}