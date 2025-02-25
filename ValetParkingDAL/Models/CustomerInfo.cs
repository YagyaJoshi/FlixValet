using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlTypes;

namespace ValetParkingDAL.Models
{
    public class CustomerInfo
    {
        public long Id { get; set; }
        [Required]
        public long UserId { get; set; }

        public bool NotifyMeBeforeBooking { get; set; }

        public DateTime? LastActiveDatetime { get; set; }
        [Required]
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string ReservationNotificationMode { get; set; }
        public string PaymentNotificationMode { get; set; }

    }
}