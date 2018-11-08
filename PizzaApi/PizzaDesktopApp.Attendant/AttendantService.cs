using System;
using System.Collections.Generic;
using GreenPipes;
using MassTransit;
using PizzaApi.MessageContracts;
using RabbitMQ.Client;
using Topshelf;

namespace PizzaDesktopApp.Attendant
{
    public class AttendantService : ServiceControl
    {
        private ConnectHandle _connectHandle;
        private IBusControl _busControl;

        public bool Start(HostControl hostControl)
        {
            var builder = new QbdTopologyBuilder();
            builder.SetupBinding("value_of_guid1");

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
                    e.Consumer<WantAllFaultsGimmeThem>();
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

    public class QbdTopologyBuilder
    {
        private const string QueueNamePrefix = "qbd.request.";

        public void SetupBinding(string fileId)
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                VirtualHost = "pizzaapi",
                Password = RabbitMqConstants.Password,
                UserName = RabbitMqConstants.UserName
            };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var queue = QueueNamePrefix + fileId;
                var mtFanoutExchange = "PizzaApi.MessageContracts:IRequestCommand";
                var ourHeaderExchange = "PizzaApi.header_IRequestCommand";

                channel.QueueDeclare(queue: queue,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                channel.ExchangeDeclare(mtFanoutExchange, "fanout", durable: true, autoDelete: false);
                channel.ExchangeDeclare(ourHeaderExchange, "headers", durable: true, autoDelete: false);

                // send all msgs from MT exchange to our exchange
                channel.ExchangeBind(destination: ourHeaderExchange, source: mtFanoutExchange, routingKey: "");

                // send some msgs from our exchange to specific queue
                channel.QueueBind(queue, ourHeaderExchange, "", new Dictionary<string, object>
                {
                    ["fileId"] = fileId
                });
            }
        }
    }
}