using System;

namespace PizzaApi.MessageContracts
{
    public interface IOrderMaxTimeExpiredEvent : MassTransit.CorrelatedBy<Guid>
    {
        DateTime Timestamp { get; }

        int OrderID { get; }
        int? EstimatedTime { get; }

        string CustomerName { get; }
        string CustomerPhone { get; }
    }
}