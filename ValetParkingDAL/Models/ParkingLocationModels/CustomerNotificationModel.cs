using System;
using System.Collections.Generic;

namespace ValetParkingDAL.Models.ParkingLocationModels
{


    public class CustomerNotificationModel
    {
        public List<CustomerNotification> Notifications { get; set; }
        public int Total { get; set; }
    }
    public class CustomerNotification
    {
        public long NotificationId { get; set; }
        public string LocationName { get; set; }
        public string Message { get; set; }
        public string LocationPic { get; set; }
        public DateTime NotificationDateTime { get; set; }

        public bool IsBookingCompleted { get; set; }

        public long UnreadCount { get; set; }

        public bool ShowActionButtons { get; set; }
    }
}