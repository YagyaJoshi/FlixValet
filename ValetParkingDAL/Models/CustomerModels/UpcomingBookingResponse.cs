using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class UpcomingBookingResponse
    {
        public List<UpcomingBookingList> UpcomingBooking { get; set; }
        public int Total { get; set; }
    }
    public class UpcomingBookingList
    {
        public long Id { get; set; }
        public string CustomerName { get; set; }

        public string Address { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string ProfilePic { get; set; }
        public decimal NetAmount { get; set; }
        public decimal ExtraCharges { get; set; }
        public decimal BookingAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public decimal Duration { get; set; }
        public string VehicleModal { get; set; }

        public string NumberPlate { get; set; }

        public DateTime? BookingStartDateTime { get; set; }

        public DateTime? BookingEndDateTime { get; set; }

    }
}