using System;
using Automatonymous;
using GreenPipes;

//using Hangfire;
//using Hangfire.MemoryStorage;
using MassTransit;
using MassTransit.BusConfigurators;
using MassTransit.Saga;
using PizzaApi.MessageContracts;
using PizzaApi.StateMachines;
using Topshelf;
using System.Linq;

using MassTransit.EntityFrameworkIntegration.Saga;
using MassTransit.EntityFrameworkIntegration;

//using System.Data.Entity;

namespace PizzaApi.WindowsService
{
    public class SagaService : ServiceControl
    {
        private IBusControl _busControl;
        private IBusObserver _busObserver;

        //private BackgroundJobServer hangfireServer;
        private readonly OrderStateMachine _saga;

        public SagaService(OrderStateMachine orderStateMachine)
        {
            _saga = orderStateMachine;
        }

        public bool Start(HostControl hostControl)
        {
            SagaDbContextFactory sagaDbContextFactory =
                () => new SagaDbContext<Order, OrderMap>("MyContext");
            var repo = new EntityFrameworkSagaRepository<Order>(sagaDbContextFactory, optimistic: true);

            _busObserver = new BusObserver();

            _busControl = BusConfigurator.ConfigureBus((cfg, host) =>
            {
                cfg.AddBusFactorySpecification(new BusObserverSpecification(() => _busObserver));

                cfg.UseSerilog();
                cfg.EnableWindowsPerformanceCounters();

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

                    //e.UseRetry(Retry.Except(typeof(ArgumentException),
                    //    typeof(NotAcceptedStateMachineException)).Interval(10, TimeSpan.FromSeconds(5)));
                    //TODO: Create a custom filter policy for inner exceptions on Sagas: http://stackoverflow.com/questions/37041293/how-to-use-masstransits-retry-policy-with-sagas

                    e.StateMachineSaga(_saga, repo);
                });
            });

            var consumeObserver = new LogConsumeObserver();

            _busControl.ConnectConsumeObserver(consumeObserver);

            //TODO: See how to do versioning of messages (best practices)
            //http://masstransit.readthedocs.io/en/master/overview/versioning.html

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

    public class OrderMap :
        SagaClassMapping<Order>
    {
        public OrderMap()
        {
            ToTable("Order2");
            Property(x => x.CurrentState)
                .HasMaxLength(64);

            Property(x => x.Created);
            Property(x => x.Updated);

            Property(x => x.CustomerName);
            Property(x => x.CustomerPhone);
            Property(x => x.EstimatedTime);
            Property(x => x.OrderID);
            Property(x => x.PizzaID);
            Property(x => x.RejectedReasonPhrase);
            Property(x => x.Status);

            // CorrelationId already mapped in base class
        }
    }

    //public class MyContext : DbContext
    //{
    //    public MyContext(string nameOrConnectionString)
    //        : base(nameOrConnectionString)
    //    {
    //    }

    //    public DbSet<Order> Orders { get; set; }

    //    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    //    {
    //        modelBuilder.Entity<Order>()
    //            .ToTable("Order2")
    //            .Property(x => x.CurrentState);

    //        modelBuilder.Entity<Order>().Property(x => x.Created);
    //        modelBuilder.Entity<Order>().Property(x => x.Updated);
    //        modelBuilder.Entity<Order>().Property(x => x.CustomerName);
    //        modelBuilder.Entity<Order>().Property(x => x.CustomerPhone);
    //        modelBuilder.Entity<Order>().Property(x => x.EstimatedTime);
    //        modelBuilder.Entity<Order>().Property(x => x.OrderID);
    //        modelBuilder.Entity<Order>().Property(x => x.PizzaID);
    //        modelBuilder.Entity<Order>().Property(x => x.RejectedReasonPhrase);
    //        modelBuilder.Entity<Order>().Property(x => x.Status);
    //        modelBuilder.Entity<Order>().Property(x => x.CorrelationId);
    //    }
    //}
}