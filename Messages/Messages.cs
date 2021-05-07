using System;

namespace Messages
{
    // 定义 message interface，也可以直接定义为 class
    public interface ISubmitOrder
    {
        string OrderNumber { get; set; }
    }


    public interface IOrderAccepted
    {
        string OrderNumber { get; set; }
    }


    public interface IOrderReceived
    {
        DateTime Timestamp { get; set; }

        string OrderNumber { get; set; }
    }
}
