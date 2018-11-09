using System;

namespace PizzaApi.MessageContracts
{
    public interface IRequestCommand : MassTransit.CorrelatedBy<Guid>
    {
        DateTime Timestamp { get; }

        int OrderID { get; }
    }

    public interface IQboRequestCommand : IRequestCommand
    {
    }

    public interface IQbdRequestCommand : IRequestCommand
    {
        string FileId { get; }
    }
}