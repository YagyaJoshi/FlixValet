using System;
using System.Collections.Generic;

namespace ValetParkingDAL.Models.JobModels
{
    public class NotificationModel
    {
        public long ParkingLocationId { get; set; }
        public long CustomerId { get; set; }
        public int NotificationTypeId { get; set; }
        public DateTime NotificationDateTime { get; set; }
        public string Message { get; set; }
        public long CustomerBookingId { get; set; }
    }

    public class NotificationListModel
    {
        public List<NotificationModel> NotificationList { get; set; }
    }
}