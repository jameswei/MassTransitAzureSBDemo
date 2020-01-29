namespace MTFunctions001
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GreenPipes;
    using MassTransit;
    using MassTransit.Context;
    using MassTransit.WebJobs.ServiceBusIntegration;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.WebJobs;
    using Messages;


    public static class Functions
    {
        [FunctionName("SubmitOrder")]
        public static async Task SubmitOrderAsync([ServiceBusTrigger("input-queue")] Message message, IBinder binder, Microsoft.Extensions.Logging.ILogger logger,
            CancellationToken cancellationToken)
        {
            
            LogContext.ConfigureCurrentLogContext(logger);
            LogContext.Info?.Log("Creating brokered message receiver");

            var handler = Bus.Factory.CreateBrokeredMessageReceiver(binder, cfg =>
            {
                cfg.CancellationToken = cancellationToken;
                cfg.InputAddress = new Uri("sb://abc-orders.servicebus.windows.net/input-queue");

                cfg.UseRetry(x => x.Intervals(10, 100, 500, 1000));
                cfg.Consumer(() => new SubmitOrderConsumer());
            });

            await handler.Handle(message);
        }
       
    }


    public class SubmitOrderConsumer :  IConsumer<ISubmitOrder>
    {
        public Task Consume(ConsumeContext<ISubmitOrder> context)
        {
            LogContext.Debug?.Log("Processing Order: {OrderNumber}", context.Message.OrderNumber);

            //context.Publish<OrderReceived>(new
            //{
            //    context.Message.OrderNumber,
            //    Timestamp = DateTime.UtcNow
            //});

            //OrderReceived orderReceived = new OrderReceived();
            //orderReceived.OrderNumber = context.Message.OrderNumber;
            //orderReceived.Timestamp = System.DateTime.Now;

            //return context.RespondAsync<OrderReceived>(new { System.DateTime.Now, context.Message.OrderNumber });
            //return context.Respond < OrderReceived > orderReceived; 
            return context.RespondAsync<IOrderReceived>(new { System.DateTime.Now, context.Message.OrderNumber });
        }
    }

    
    public class OrderReceived : Messages.IOrderReceived
    {
         public DateTime Timestamp { get; set; }
         public string OrderNumber { get; set; }
    }
}
