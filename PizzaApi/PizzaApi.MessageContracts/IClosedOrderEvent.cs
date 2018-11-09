using System;

namespace PizzaApi.MessageContracts
{
    public interface IClosedOrderEvent : MassTransit.CorrelatedBy<Guid>
    {
        DateTime Timestamp { get; }

        int OrderID { get; }
        int Status { get; }
    }
}