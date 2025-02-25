using System;
using System.ComponentModel.DataAnnotations;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class PostBookingSaveModel
    {
        [Required]
        public long ParkingLocationId { get; set; }
        [Required]
        public long CustomerId { get; set; }

        public string CustomerNotificationType { get; set; }
        public string ValetNotificationType { get; set; }
        [Required]
        public DateTime NotificationDateTime { get; set; }
        public string CustomerMessage { get; set; }
        public string ValetMessage { get; set; }
        public long CustomerBookingId { get; set; }
        public string QRCodePath { get; set; }
        public bool SaveValetNotification { get; set; }

        public bool SaveNotification { get; set; }
    }
}