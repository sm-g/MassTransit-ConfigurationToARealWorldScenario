using System;

namespace PizzaApi.MessageContracts
{
    public interface IDomainOperationRequest : MassTransit.CorrelatedBy<Guid>
    {
        DateTime Timestamp { get; }

        int Question { get; }
    }
}