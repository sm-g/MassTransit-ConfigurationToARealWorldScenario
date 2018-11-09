using System;

namespace PizzaApi.MessageContracts
{
    public interface INextWorkCommand : MassTransit.CorrelatedBy<Guid>
    {
        DateTime Timestamp { get; }

        int OrderID { get; }
    }
}