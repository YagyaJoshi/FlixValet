using System;
namespace ValetParkingDAL.Models.CustomerModels
{
    public class NotificationResponse
    {

        public long ParkingLocationId { get; set; }
        public long CustomerId { get; set; }
        public string NotificationType { get; set; }
        public DateTime NotificationDateTime { get; set; }
        public string Message { get; set; }
        public DateTime CreatedDate { get; set; }

    }
}