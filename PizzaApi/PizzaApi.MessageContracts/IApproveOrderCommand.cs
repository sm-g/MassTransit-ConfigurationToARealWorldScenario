using System;

namespace PizzaApi.MessageContracts
{
    public interface IApproveOrderCommand : MassTransit.CorrelatedBy<Guid>
    {
        DateTime Timestamp { get; }

        int OrderID { get; }
        int? EstimatedTime { get; }
        int Status { get; }
    }
}