using MassTransit;
using MassTransit.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaApi.MessageContracts
{
    public class BusObserver : IBusObserver
    {
        public Task CreateFaulted(Exception exception)
        {
            return Task.Run(() => Logger.Get("sagaBusObserver").InfoFormat("CreateFaulted"));
        }

        public Task PostCreate(IBus bus)
        {
            return Task.Run(() => Logger.Get("sagaBusObserver").Debug(() => "PostCreate"));
        }

        public Task PostStart(IBus bus, Task<BusReady> busReady)
        {
            return Task.Run(() => Logger.Get("sagaBusObserver").Info(() => "PostStart"));
        }

        public Task PostStop(IBus bus)
        {
            return Task.Run(() => Logger.Get("sagaBusObserver").InfoFormat("PostStop"));
        }

        public Task PreStart(IBus bus)
        {
            return Task.Run(() => Logger.Get("sagaBusObserver").InfoFormat("PreStart"));
        }

        public Task PreStop(IBus bus)
        {
            return Task.Run(() => Logger.Get("sagaBusObserver").InfoFormat("PreStop"));
        }

        public Task StartFaulted(IBus bus, Exception exception)
        {
            return Task.Run(() => Logger.Get("sagaBusObserver").InfoFormat("StartFaulted"));
        }

        public Task StopFaulted(IBus bus, Exception exception)
        {
            return Task.Run(() => Logger.Get("sagaBusObserver").InfoFormat("StopFaulted"));
        }
    }
}