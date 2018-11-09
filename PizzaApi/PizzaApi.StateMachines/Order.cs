using System;
using Automatonymous;

namespace PizzaApi.StateMachines
{
    public class Order : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }

        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public int? OrderID { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public int? EstimatedTime { get; set; }
        public int Status { get; set; }
        public string RejectedReasonPhrase { get; set; }
        public int PizzaID { get; set; }
        public Guid? DomainOperationRequestId { get; set; }
    }
}