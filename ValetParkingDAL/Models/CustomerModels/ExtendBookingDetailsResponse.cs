using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class ExtendBookingDetailsResponse
    {
        public bool IsBookingExists { get; set; }
        public ParkingLocationDetails ParkingLocationDetails { get; set; }
        public List<CustomerVehicleList> CustomerVehicles { get; set; }

        public BookingDetailsResponse BookingDetails { get; set; }
    }
    public class BookingDetailsResponse
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public long ExistingVehicleId { get; set; }

    }
}