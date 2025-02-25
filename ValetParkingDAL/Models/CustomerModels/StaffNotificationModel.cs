using System;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class StaffNotificationModel
    {
        public long? UserId { get; set; }
        public long NotificationId { get; set; }
        public bool IsFromCustomer { get; set; }
        public string NotificationMessage { get; set; }
        public DateTime NotificationDateTime { get; set; }
    }
}