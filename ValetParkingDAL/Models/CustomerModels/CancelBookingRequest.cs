using System;
using System.ComponentModel.DataAnnotations;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class CancelBookingRequest
    {
        [Required]
        public long BookingId { get; set; }

        public DateTime CurrentDate { get; set; }
    }
}