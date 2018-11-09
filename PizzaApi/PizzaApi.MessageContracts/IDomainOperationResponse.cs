using System;

namespace PizzaApi.MessageContracts
{
    public interface IDomainOperationResponse : MassTransit.CorrelatedBy<Guid>
    {
        DateTime Timestamp { get; }

        int Answer { get; }
    }
}