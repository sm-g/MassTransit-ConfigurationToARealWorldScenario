using System;

namespace PizzaApi.MessageContracts
{
    //TODO: Remove it later
    public interface IOrderApprovedEvent : MassTransit.CorrelatedBy<Guid>
    {
        DateTime Timestamp { get; }

        int OrderID { get; }
        int? EstimatedTime { get; }

        int Status { get; }
    }
}