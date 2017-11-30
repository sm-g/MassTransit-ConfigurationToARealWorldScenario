﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaApi.MessageContracts
{
    public interface IRegisterOrderCommand
    {
        Guid CorrelationId { get; }
        DateTime Timestamp { get; }

        int OrderID { get; }
        string CustomerName { get; }
        string CustomerPhone { get; }
        int PizzaID { get; }
    }

    public interface IParallelWorkCommand
    {
        Guid CorrelationId { get; }
        DateTime Timestamp { get; }

        int OrderID { get; }
    }

    public interface INextWorkCommand
    {
        Guid CorrelationId { get; }
        DateTime Timestamp { get; }

        int OrderID { get; }
    }
}