using MassTransit;
using PizzaApi.MessageContracts;
using System;
using System.Threading.Tasks;

namespace PizzaDesktopApp.Attendant
{
    public class WantAllFaultsGimmeThem : IConsumer<Fault>
    {
        public async Task Consume(ConsumeContext<Fault> context)
        {
            var fault = context.Message;
            if (context.TryGetMessage<Fault<IOrderRegisteredEvent>>(out var faultContext))
            {
                Console.WriteLine(faultContext);
            }

            Console.WriteLine();
        }
    }
}