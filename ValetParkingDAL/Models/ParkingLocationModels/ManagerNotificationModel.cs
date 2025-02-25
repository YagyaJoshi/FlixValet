using System.Collections.Generic;

namespace ValetParkingDAL.Models.ParkingLocationModels
{
    public class ManagerNotificationModel
    {
        public List<ManagerNotification> Notifications { get; set; }
        public int Total { get; set; }
    }

    public class ManagerNotification
    {
        public long NotificationId { get; set; }
        public string CustomerName { get; set; }
        public string Message { get; set; }
        public string ProfilePic { get; set; }
        public bool IsAccepted { get; set; }
        public long? AcceptedUserId { get; set; }
        public bool IsBookingCompleted { get; set; }

        public long UnreadCount { get; set; }

        public bool ShowActionButtons { get; set; }

    }
}