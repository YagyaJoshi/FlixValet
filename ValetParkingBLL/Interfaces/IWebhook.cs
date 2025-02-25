using System;
using System.Collections.Generic;
using System.Text;

namespace ValetParkingBLL.Interfaces
{
    public interface IWebhook
    {
        bool UpdateCustomerAmount(string CustomerId, decimal amount);
    }
}
