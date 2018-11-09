using System;
using System.Threading.Tasks;
using MassTransit;
using PizzaApi.MessageContracts;

namespace PizzaDesktopApp.Attendant
{
    public class DomainOperationRequestConsumer : IConsumer<IDomainOperationRequest>
    {
        public Task Consume(ConsumeContext<IDomainOperationRequest> context)
        {
            if (context.Message.Question == -1)
                throw new ArgumentException("Cannot answer to -1 question");

            return context.RespondAsync(new Response
            {
                CorrelationId = context.Message.CorrelationId
            });
        }

        private class Response : IDomainOperationResponse
        {
            public DateTime Timestamp { get; set; } = DateTime.UtcNow;
            public int Answer { get; set; } = 42;
            public Guid CorrelationId { get; set; }
        }
    }
}