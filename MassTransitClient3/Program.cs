
namespace MassTransitClient2
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    //using log4net.Config;
    using MassTransit;
    using MassTransit.Azure.ServiceBus.Core;
    //using MassTransit.Log4NetIntegration.Logging;
    using MassTransit.Util;
    using Messages;
    using Microsoft.Azure.ServiceBus.Primitives;

    class Program
    {
        static void Main()
        {
            //ConfigureLogger();

            // MassTransit to use Log4Net
            //Log4NetLogger.Use();

            // 配置 MassTransit 的 service bus
            IBusControl busControl = CreateBus();

            TaskUtil.Await(() => busControl.StartAsync());

            try
            {
                var client = CreateRequestClient(busControl);

                for (; ; )
                {
                    Console.Write("Enter customer id, 'quit to exit'");
                    string customerId = Console.ReadLine();
                    if (customerId == "quit")
                    {
                        break;
                    }

                    // this is run as a Task to avoid weird console application issues
                    Task.Run(async () =>
                    {
                        IOrderReceived response = await client.Request(new SubmitOrder { OrderNumber = customerId });
                        Console.WriteLine("Customer Name: {0}", response.OrderNumber + ": Name");
                    }).Wait();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception!!! {0}", ex);
            }
            finally
            {
                busControl.Stop();
            }
        }


        // 通过 IBusControl 创建 sender
        static IRequestClient<ISubmitOrder, IOrderReceived> CreateRequestClient(IBusControl bus)
        {

            var client = bus.CreateRequestClient<ISubmitOrder, IOrderReceived>(new Uri("sb://abc-orders.servicebus.windows.net/input-queue"), TimeSpan.FromSeconds(30));

            return client;
        }

        static IBusControl CreateBus()
        {

            // 使用 ASB 作为 transport provider 来创建 MassTransit 的 service bus
            return Bus.Factory.CreateUsingAzureServiceBus(cfg =>
            {
                cfg.Host(new Uri("sb://abc-orders.servicebus.windows.net"), host =>
                {
                    host.OperationTimeout = TimeSpan.FromSeconds(5);
                    host.TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider("MassTransitClient", "YOUR SHARED ACCESS KEY HERE");
                });
            });

        }

        static void ConfigureLogger()
        {
            const string logConfig = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
                <log4net>
                  <root>
                    <level value=""INFO"" />
                    <appender-ref ref=""console"" />
                  </root>
                  <appender name=""console"" type=""log4net.Appender.ColoredConsoleAppender"">
                    <layout type=""log4net.Layout.PatternLayout"">
                      <conversionPattern value=""%m%n"" />
                    </layout>
                  </appender>
                </log4net>";

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(logConfig)))
            {
                //XmlConfigurator.Configure(stream);
            }
        }

    }


    public class SubmitOrder : Messages.ISubmitOrder
    {
        public string OrderNumber { get; set; }
    }
    public class OrderAccepted : Messages.IOrderAccepted
    {
        public string OrderNumber { get; set; }
    }
    public class OrderReceived : Messages.IOrderReceived
    {
        public DateTime Timestamp { get; set; }
        public string OrderNumber { get; set; }
    }
}