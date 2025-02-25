using System;
using System.ComponentModel.DataAnnotations;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class CurrentLocationRequest
    {

        public string Latitude { get; set; }

        public string Longitude { get; set; }
        public int RDistance { get; set; }
        public string WeekDay { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string BookingType { get; set; }

        [ValidateDateTime]
        public string StartTime { get; set; }
        [ValidateDateTime]

        public string EndTime { get; set; }
        public string TimeZone { get; set; }

        public string TimeZoneId { get; set; }
        public long ParkingLocationId { get; set; }
        public bool IsFullTimeBooking { get; set; }

    }
}