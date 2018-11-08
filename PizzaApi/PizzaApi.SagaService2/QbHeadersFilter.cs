using GreenPipes;
using MassTransit;
using PizzaApi.MessageContracts;
using System.Threading.Tasks;

namespace PizzaApi.SagaService2
{
    public class QbHeadersFilter<T, TMessage> : IFilter<T>
        where T : class, SendContext<TMessage>
        where TMessage : class
    {
        public void Probe(ProbeContext context)
        {
            // empty
        }

        public Task Send(T context, IPipe<T> next)
        {
            if (context.Message is IQbdRequestCommand command)
            {
                context.Headers.Set("fileId", command.FileId);
                context.Headers.Set("qbType", "qbd");
            }
            if (context.Message is IQboRequestCommand)
            {
                context.Headers.Set("qbType", "qbo");
            }

            return next.Send(context);
        }
    }
}