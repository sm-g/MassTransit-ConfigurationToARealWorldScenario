using System;
using Automatonymous;
using MassTransit.Logging;
using Newtonsoft.Json;
using PizzaApi.MessageContracts;

namespace PizzaApi.StateMachines
{
    public class OrderStateMachine : MassTransitStateMachine<Order>
    {
        public OrderStateMachine(IServiceProvider serviceProvider)
        {
            Logger.Get<OrderStateMachine>().InfoFormat("OrderStateMachine ctor");

            InstanceState(x => x.CurrentState);

            Event(() => RegisterOrder,
                cc => cc.CorrelateBy(order => order.OrderID,
                                    context => context.Message.OrderID)
                        .SelectId(context => context.Message.CorrelationId));

            Event(() => DoParallelWork, cc => cc.CorrelateById(context => context.Message.CorrelationId));
            Event(() => DoNextWork, cc => cc.CorrelateById(context => context.Message.CorrelationId));
            Event(() => ApproveOrder, cc => cc.CorrelateById(context => context.Message.CorrelationId));
            Event(() => CloseOrder, cc => cc.CorrelateById(context => context.Message.CorrelationId));
            Event(() => RejectOrder, cc => cc.CorrelateById(context => context.Message.CorrelationId));

            Initially(
                When(RegisterOrder)
                    .Then(context =>
                    {
                        //throw new ArgumentException("Test for monitoring sagas");

                        context.Instance.Created = context.Data.Timestamp;
                        context.Instance.OrderID = context.Data.OrderID;
                        context.Instance.CustomerName = context.Data.CustomerName;
                        context.Instance.CustomerPhone = context.Data.CustomerPhone;
                        context.Instance.PizzaID = context.Data.PizzaID;

                        Logger.Get<OrderStateMachine>().InfoFormat("Register Order {0}", JsonConvert.SerializeObject(context.Instance));
                    })
                    .TransitionTo(Registered)
                    //.Publish(context => new OrderRegisteredEvent(context.Instance))
                    .Publish(context => true ? new ParallelWorkCommand(context.Instance) : throw new Exception())
                //.Publish(context => new QbdRequestCommand(context.Instance))
                //.Publish(context => new NextWorkCommand(context.Instance))
                );

            During(Registered,
                //Ignore(DoParallelWork),
                When(DoParallelWork)
                    .Then(context =>
                    {
                        // only StateMachine executes next, not Publish
                        context.Raise(DoNextWork, new NextWorkCommand(context.Instance));
                        Logger.Get<OrderStateMachine>().InfoFormat("Doing parallel work for Order {0}", context.Instance.OrderID);
                    })
                    .Catch<ArgumentNullException>(ex => ex.Publish(context => new ParallelWorkCommand(context.Instance)))
                    .Publish(context => new NextWorkCommand(context.Instance))
                    ,
                When(DoNextWork)
                    .Then(context =>
                    {
                        Logger.Get<OrderStateMachine>().InfoFormat("Doing next work for Order {0}", context.Instance.OrderID);
                    })
                    .Publish(context => new OrderRegisteredEvent(context.Instance))
                    ,
                When(ApproveOrder)
                    .Then(context =>
                    {
                        //throw new ArgumentException("Test for monitoring sagas");

                        context.Instance.Updated = context.Data.Timestamp;
                        context.Instance.EstimatedTime = context.Data.EstimatedTime;
                        context.Instance.Status = context.Data.Status;

                        var delayedTimeInSeconds = context.Instance.EstimatedTime.Value * 60 * 0.65f;
                        Console.WriteLine("delayedTime (in seconds): " + delayedTimeInSeconds);
                        //BackgroundJob.Schedule(() => Console.WriteLine("Send notification to client: Pay attention please. Your order is near to be done!"),
                        //                                TimeSpan.FromSeconds(delayedTimeInSeconds));

                        Logger.Get<OrderStateMachine>().InfoFormat("Approve Order {0}", JsonConvert.SerializeObject(context.Instance));
                    })
                    .ThenAsync(async context =>
                    {
                        //throw new ArgumentException("Test for monitoring sagas");

                        await Console.Out.WriteLineAsync(string.Format("Send notification to client {0} with order id: {1} about your order status 'APPROVED'.",
                                                                                                context.Instance.CustomerName, context.Instance.OrderID));
                    })
                    .TransitionTo(Approved),
                //.Publish(context => new OrderApprovedEvent(context.Instance))//In this scenario, i don´t need of this event...
                When(RejectOrder)
                    .Then(context =>
                    {
                        context.Instance.Updated = context.Data.Timestamp;
                        context.Instance.RejectedReasonPhrase = context.Data.RejectedReasonPhrase;

                        Logger.Get<OrderStateMachine>().InfoFormat("Reject Order {0}", JsonConvert.SerializeObject(context.Instance));
                    })
                    .ThenAsync(async context => await Console.Out.WriteLineAsync(string.Format("Send notification to client {0} with order id {1} about your order status 'REJECTED', reason: {2}.",
                                                                                                context.Instance.CustomerName, context.Instance.OrderID, context.Instance.RejectedReasonPhrase)))
                    .Finalize()
                );

            During(Approved,
                When(CloseOrder)
                    .Then(context =>
                    {
                        //throw new ArgumentException("Test for monitoring sagas");
                        context.Instance.Updated = context.Data.Timestamp;
                        context.Instance.Status = context.Data.Status;

                        Logger.Get<OrderStateMachine>().InfoFormat("Close Order {0}", JsonConvert.SerializeObject(context.Instance));
                    })
                    .ThenAsync(async context => await Console.Out.WriteLineAsync(string.Format("Send notification to client {0} with order id: {1} about your order status 'CLOSED'",
                                                                                                context.Instance.CustomerName, context.Instance.OrderID)))
                    .Finalize()
                );

            SetCompletedWhenFinalized();
        }

        public State Registered { get; private set; }
        public State Approved { get; private set; }

        //Should add Closed state?
        public Event<IRegisterOrderCommand> RegisterOrder { get; private set; }

        public Event<IParallelWorkCommand> DoParallelWork { get; private set; }
        public Event<INextWorkCommand> DoNextWork { get; private set; }

        public Event<IApproveOrderCommand> ApproveOrder { get; private set; }
        public Event<ICloseOrderCommand> CloseOrder { get; private set; }
        public Event<IRejectOrderCommand> RejectOrder { get; private set; }

        public Event NoDataEvent { get; private set; }
    }

    public class ParallelWorkCommand : IParallelWorkCommand
    {
        public Guid CorrelationId { get; }
        public DateTime Timestamp { get; }
        public int OrderID { get; }

        public ParallelWorkCommand(Order orderInstance)
        {
            CorrelationId = orderInstance.CorrelationId;
            Timestamp = orderInstance.Updated;

            OrderID = orderInstance.OrderID.Value;
        }
    }

    public class NextWorkCommand : INextWorkCommand
    {
        public Guid CorrelationId { get; }
        public DateTime Timestamp { get; }
        public int OrderID { get; }

        public NextWorkCommand(Order orderInstance)
        {
            CorrelationId = orderInstance.CorrelationId;
            Timestamp = orderInstance.Updated;

            OrderID = orderInstance.OrderID.Value;
        }
    }

    public class QbdRequestCommand : IQbdRequestCommand
    {
        public Guid CorrelationId { get; }
        public DateTime Timestamp { get; }
        public int OrderID { get; }
        public string FileId { get; }

        public QbdRequestCommand(Order orderInstance)
        {
            CorrelationId = orderInstance.CorrelationId;
            Timestamp = orderInstance.Updated;

            OrderID = orderInstance.OrderID.Value;
            FileId = "value_of_guid1";
        }
    }

    public class QboRequestCommand : IQboRequestCommand
    {
        public Guid CorrelationId { get; }
        public DateTime Timestamp { get; }
        public int OrderID { get; }

        public QboRequestCommand(Order orderInstance)
        {
            CorrelationId = orderInstance.CorrelationId;
            Timestamp = orderInstance.Updated;

            OrderID = orderInstance.OrderID.Value;
        }
    }
}