using System;
using System.ComponentModel.DataAnnotations;

namespace ValetParkingDAL.Models
{
    public class Notification
    {

        [Required]
        public long ParkingLocationId { get; set; }
        [Required]
        public long CustomerId { get; set; }
        public string NotificationType { get; set; }
        [Required]
        public DateTime NotificationDateTime { get; set; }
        public string Message { get; set; }

        public long CustomerBookingId { get; set; }

    }
}