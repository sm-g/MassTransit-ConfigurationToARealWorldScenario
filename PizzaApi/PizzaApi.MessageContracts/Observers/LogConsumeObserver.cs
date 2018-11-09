using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Logging;
using Newtonsoft.Json;

namespace PizzaApi.MessageContracts
{
    public class LogConsumeObserver : IConsumeObserver
    {
        //private static Logger logger = LogManager.GetCurrentClassLogger();
        public Task ConsumeFault<T>(ConsumeContext<T> context, Exception exception) where T : class
        {
            var messageContext = JsonConvert.SerializeObject(context.Message);

            //TODO: Modify all consumer's observers to use log format corretly...
            return Task.Run(() => Logger.Get("mongoCustomLog").ErrorFormat("Error on CONSUME: " + messageContext + "with exception: " + exception.Message));
        }

        public Task PostConsume<T>(ConsumeContext<T> context) where T : class
        {
            var messageContext = JsonConvert.SerializeObject(context.Message);

            return Task.Run(() => Logger.Get("mongoCustomLog").InfoFormat("ConsumeObserver - PostConsume Observed with context: " + messageContext));
        }

        public Task PreConsume<T>(ConsumeContext<T> context) where T : class
        {
            var messageContext = JsonConvert.SerializeObject(context.Message);

            return Task.Run(() => Logger.Get("mongoCustomLog").InfoFormat("ConsumeObserver - PreConsume Observed with context: {0}", messageContext));
        }
    }
}