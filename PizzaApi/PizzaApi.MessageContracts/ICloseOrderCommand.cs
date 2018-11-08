using System;

namespace PizzaApi.MessageContracts
{
    public interface ICloseOrderCommand
    {
        Guid CorrelationId { get; }
        DateTime Timestamp { get; }

        int OrderID { get; }
        int Status { get; }
    }
}