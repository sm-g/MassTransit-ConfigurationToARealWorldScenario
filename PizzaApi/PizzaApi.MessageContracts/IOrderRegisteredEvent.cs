using System;

namespace PizzaApi.MessageContracts
{
    public interface IOrderRegisteredEvent : MassTransit.CorrelatedBy<Guid>
    {
        DateTime Timestamp { get; }

        int OrderID { get; }
        string CustomerName { get; }
        string CustomerPhone { get; }
        int PizzaID { get; }
    }
}