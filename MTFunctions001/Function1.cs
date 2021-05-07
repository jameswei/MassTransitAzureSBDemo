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
    using Microsoft.Extensions.Logging;
    using Messages;


    public static class Functions
    {
        // 定义 Azure Functions method
        [FunctionName("SubmitOrder")]
        public static async Task SubmitOrderAsync(
            // 指定 trigger type
            [ServiceBusTrigger("input-queue")]
            Message message,
            IBinder binder, ILogger logger, CancellationToken cancellationToken)
        {

            LogContext.ConfigureCurrentLogContext(logger);
            LogContext.Info?.Log("Creating brokered message receiver");

            var handler = Bus.Factory.CreateBrokeredMessageReceiver(binder, cfg =>
            {
                cfg.CancellationToken = cancellationToken;
                cfg.InputAddress = new Uri("sb://abc-orders.servicebus.windows.net/input-queue");
                // 设置 retry 策略
                cfg.UseRetry(x => x.Intervals(10, 100, 500, 1000));
                // 指定要转发消息的 consumer
                cfg.Consumer(() => new SubmitOrderConsumer());
            });

            await handler.Handle(message);
        }

    }

    // 实现 IConsumer 的 consumer
    public class SubmitOrderConsumer : IConsumer<ISubmitOrder>
    {
        public Task Consume(ConsumeContext<ISubmitOrder> context)
        {
            LogContext.Debug?.Log("Processing Order: {OrderNumber}", context.Message.OrderNumber);

            return context.RespondAsync<IOrderReceived>(new { System.DateTime.Now, context.Message.OrderNumber });
        }
    }


    public class OrderReceived : Messages.IOrderReceived
    {
        public DateTime Timestamp { get; set; }
        public string OrderNumber { get; set; }
    }
}
