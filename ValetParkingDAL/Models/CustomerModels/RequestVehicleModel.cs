using System;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class RequestVehicleModel
    {
        public long CustomerId { get; set; }

        public DateTime NotificationDateTime { get; set; }

        public long? CustomerVehicleId { get; set; }
    }
}