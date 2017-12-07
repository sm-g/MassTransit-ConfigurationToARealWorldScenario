using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaApi.MessageContracts
{
    public interface IRequestCommand
    {
        Guid CorrelationId { get; }
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