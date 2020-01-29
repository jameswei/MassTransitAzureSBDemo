using System;

namespace Messages
{


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
