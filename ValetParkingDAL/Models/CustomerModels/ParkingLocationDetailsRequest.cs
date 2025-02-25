using System;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class ParkingLocationDetailsRequest
    {
        public long ParkingLocationId { get; set; }
        public long CustomerId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public bool BookingType { get; set; }

    }
}