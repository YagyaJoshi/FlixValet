using System;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class BookingDetails
    {
        public double Duration { get; set; }
        public decimal Charges { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StartDateUtc { get; set; }
        public DateTime EndDateUtc { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}