using PizzaApi.StateMachines;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PizzaApi.SagaService2
{
    public class OrderMap : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> entityTypeBuilder)
        {
            entityTypeBuilder.ToTable("Order2");
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