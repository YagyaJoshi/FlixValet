using System;
using System.ComponentModel.DataAnnotations;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class GuestPreBookingRequest
    {
        public long ParkingLocationId { get; set; }
        public long CustomerVehicleId { get; set; }

        public int VehicleTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        [Required]
        public string BookingType { get; set; }
        [Required]
        [ValidateDateTime]
        public string StartTime { get; set; }
        [Required]
        [ValidateDateTime]
        public string EndTime { get; set; }
        public string TimeZone { get; set; }
        [Required]
        public string TimeZoneId { get; set; }

        public bool IsFullTimeBooking { get; set; }
        public string NumberPlate { get; set; }
    }
}