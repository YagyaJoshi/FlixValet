using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class FetchCustomerDetailsRequest
    {

        [Required]
        public string Mobile { get; set; }
        [Required]
        public string Email { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        [ValidateDateTime]
        public string StartTime { get; set; }
        [ValidateDateTime]
        public string EndTime { get; set; }
        [Required]
        public long ParkingLocationId { get; set; }
        public bool IsFullTimeBooking { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public long? CustomerVehicleId { get; set; }
        [Required]
        public string BookingType { get; set; }
        public long? CustomerId { get; set; }
        public string TimeZoneId { get; set; }
        public string TimeZone { get; set; }
    }
}