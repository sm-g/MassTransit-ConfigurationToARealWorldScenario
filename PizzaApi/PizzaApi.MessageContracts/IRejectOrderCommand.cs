﻿using System;

namespace PizzaApi.MessageContracts
{
    public interface IRejectOrderCommand : MassTransit.CorrelatedBy<Guid>
    {
        DateTime Timestamp { get; }

        int OrderID { get; }
        string RejectedReasonPhrase { get; }
    }
}